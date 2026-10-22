# NicknameGenerator

[![CI](https://github.com/Humble-proger/NicknameGenerator/actions/workflows/ci.yml/badge.svg)](https://github.com/Humble-proger/NicknameGenerator/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Humble-proger.NicknameGenerator.svg)](https://www.nuget.org/packages/Humble-proger.NicknameGenerator)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Deterministic, collision-resistant nickname generator for .NET and Unity.**

The same `Guid` always produces the same nickname — across processes, machines,
and platforms. Works in ASP.NET Core, console apps, Worker Service, and Unity
(`netstandard2.0`).

```
550e8400-e29b-41d4-a716-446655440000  →  FrostyElk_00z5ReQiH
550e8400-e29b-41d4-a716-446655440000  →  FrostyElk_00z5ReQiH   (same, always)
```

---

## Install

```
dotnet add package Humble-proger.NicknameGenerator
dotnet add package Humble-proger.NicknameGenerator.DependencyInjection   # for ASP.NET / DI
```

For Unity: build the `NicknameGenerator` project, or install via NuGetForUnity.
Targets `netstandard2.0` — works in Unity 2019.4+.

---

## Quick start

### ASP.NET Core / Worker Service

```csharp
// Program.cs
builder.Services.AddNicknameGenerator(builder.Configuration);

// appsettings.json
{
  "Nickname": {
    "SecretSalt": "<32+ random bytes as base64 or plain string>"
  }
}

// Endpoint
app.MapGet("/me", (INicknameGenerator gen, Guid userId) => gen.Generate(userId));
```

Or configure programmatically:

```csharp
builder.Services.AddNicknameGenerator(o =>
{
    o.SecretSalt = builder.Configuration["NICKNAME_SALT"];
    o.WordListVersion = 1;
    o.SuffixBytes = 6;
    o.Separator = "_";
});
```

### Console app (no DI)

```csharp
using NicknameGenerator;
using NicknameGenerator.Options;

var gen = new HmacNicknameGenerator(new NicknameOptions
{
    SecretSalt = "0123456789abcdef0123456789abcdef",  // 32+ bytes
});

string nickname = gen.Generate(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
// → "FrostyElk_00z5ReQiH"
```

### Unity

```csharp
using NicknameGenerator;
using NicknameGenerator.Options;

var gen = new HmacNicknameGenerator(new NicknameOptions { SecretSalt = salt });
label.text = gen.Generate(userId);
```

No `Microsoft.Extensions.*` dependencies. Drop the DLL in `Assets/Plugins/`
(or install via NuGetForUnity).

### Non-deterministic (preview / regenerate button)

```csharp
var gen = new RandomNicknameGenerator(new NicknameOptions());
string preview = gen.Generate();   // different every call, no salt needed
```

### Handling collisions

Nickname space is huge (`60 × 60 × 2^(8·SuffixBytes)`), but in a multi-tenant
system "already taken" can still happen — usually because users pick nicknames
themselves. Deterministic retry pattern:

```csharp
string nick = nicknameGenerator.GenerateWithRetry(
    userId,
    candidate => !db.Users.Any(u => u.Nickname == candidate));

await db.Users.AddAsync(new User { Id = userId, Nickname = nick });
```

If you prefer to control the loop yourself:

```csharp
for (int attempt = 0; attempt < 16; attempt++)
{
    string candidate = nicknameGenerator.Generate(userId, attempt);
    if (await IsAvailableAsync(candidate))
    {
        return candidate;
    }
}
throw new InvalidOperationException("No free nickname");
```

⚠️ **If you use `attempt > 0`, persist the attempt value alongside the user.**
`Generate(userId)` returns the `attempt = 0` nickname, which may differ from
the one you stored on retry #3.

---

## How it works

```
       Guid userId (16 bytes)
              │
              ▼
       HMAC-SHA256(salt, guid)  →  32-byte hash
              │
       ┌──────┴──────┬────────────┐
       ▼             ▼            ▼
   hash[0..1]    hash[2..3]   hash[4..4+N]
   big-endian    big-endian   big-endian
       │             │            │
   % 60 adj      % 60 noun    Base62 suffix
   (adjectives)  (nouns)      (N bytes, padded)
       │             │            │
       └─────────────┴────────────┘
                     │
                     ▼
            "BraveFalcon_a3Kx9Lm"
```

Key properties:

- **Deterministic:** same input, same output — always, everywhere.
- **Cross-platform:** byte order is explicit (big-endian), not platform-dependent.
- **HMAC:** changing the salt changes **all** outputs; without the salt, the
  Guid → nickname mapping is not guessable.
- **Collision-resistant:** 48–80 bits of suffix entropy, see table below.

---

## Collision probability

Total space = `60 adjectives × 60 nouns × 62^N`, where `N = SuffixBytes`.

Probability of **at least one** collision among `U` users (birthday bound):

| `SuffixBytes` | Suffix length | Space | 10⁴ users | 10⁶ users | 10⁸ users |
|---|---|---|---|---|---|
| 5 | 7 chars | 3.96 × 10¹⁵ | 0.0000013% | 0.013% | **72%** ⚠️ |
| **6** (default) | 9 chars | 1.01 × 10¹⁸ | ~0% | 0.000049% | 0.49% |
| 7 | 10 chars | 2.59 × 10²⁰ | ~0% | ~0.00000019% | 0.0019% |
| 8 | 11 chars | 6.64 × 10²² | ~0% | ~0% | 0.0000075% |

**Rule of thumb:** `SuffixBytes = 6` is fine up to ~10⁷ users (0.00005%
collision probability). For 10⁸+ users, use `SuffixBytes = 7` or `8`.

Note: with `SuffixBytes = 5`, the suffix encodes 40 bits into 7 base62 chars.
Since `62^6 < 2^40 < 62^7`, the first character takes values `'0'`..`'9'` and
`'a'`..`'j'` (20 possibilities) — no entropy is lost, only the encoding is
slightly non-uniform. With `SuffixBytes = 6` (default), the first character
is always `'0'` or `'1'` for the same reason, and the remaining 8 characters
carry the full 48 bits. This is by design and covered by golden-value tests.

---

## Configuration

| Property | Default | Notes |
|---|---|---|
| `SecretSalt` | *(required)* | ≥32 bytes when UTF-8 encoded. Rotating it changes **all** outputs. |
| `WordListVersion` | `1` | Bump **only** when adding a new frozen word list. Never edit an existing version. |
| `SuffixBytes` | `6` | `5`–`10`. Higher = fewer collisions, longer nicknames. |
| `Separator` | `"_"` | Anything you want; `"_"`, `"-"`, or `""` are common. |

---

## Word lists and versioning

Word lists are **embedded in the DLL** and **frozen per version**:

- `Resources/words-en-v1.txt` — 60 adjectives, 60 nouns, never modified.
- Adding words = new file `words-en-v2.txt` + `NicknameOptions.WordListVersion = 2`.
- Users on `v1` keep their nicknames forever. Users switching to `v2` get
  new ones — but **only on their next regeneration**.

Golden-value tests in `tests/NicknameGenerator.Tests/GoldenValuesTests.cs`
freeze 8 known `Guid → nickname` pairs. If any of them changes, **it's a
breaking change** — bump `WordListVersion` and regenerate.

Custom word lists:

```csharp
public sealed class MyWordListProvider : IWordListProvider
{
    public WordList Get(int version) => version switch
    {
        1 => new WordList(1, myAdjectives, myNouns),
        _ => throw new NotSupportedException()
    };
}

var gen = new HmacNicknameGenerator(options, new MyWordListProvider());
```

---

## Breaking-change policy

While on `0.x.y`, breaking changes may appear in **minor** versions (SemVer §4).
After `1.0.0`, only major versions.

A "breaking change" for this library means **any change to a nickname produced
for an existing `Guid`** — algorithm, byte order, Base62 alphabet, word list
contents, default separator, or salt handling. Such changes are **always**
accompanied by a `CHANGELOG.md` entry and a new `WordListVersion`.

---

## Security

- `SecretSalt` should be **at least 32 bytes** from `RandomNumberGenerator`,
  stored in Key Vault / Secrets Manager — not in source control.
- Salt compromise: attacker can compute the Guid → nickname mapping. If your
  nicknames are public identifiers, rotate the salt (breaking change) or use
  a per-user salt (out of scope for this library).
- This library is **not** a password hasher. Use `Rfc2898DeriveBytes` /
  `Argon2` / `bcrypt` for that.

---

## License

MIT — see [LICENSE](LICENSE).

---

## Contributing

Issues and PRs welcome. Please ensure:

- `dotnet test` is green.
- `dotnet format --verify-no-changes` passes.
- Golden-value tests are **never** modified without a `WordListVersion` bump.
