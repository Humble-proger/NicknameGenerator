# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2026-10-22

### Added

- `IRetryableNicknameGenerator` — extends `INicknameGenerator` with a
  deterministic `attempt` index for collision-retry:
  - `Generate(Guid userId, int attempt)` — `attempt = 0` is backward-compatible
    with the original algorithm; each `attempt > 0` produces an independent,
    deterministic nickname from the same space.
  - `GenerateParts(Guid userId, int attempt)` — same, but returns `NicknameParts`.
- `RetryableNicknameGeneratorExtensions.GenerateWithRetry` — convenience for the
  common "try until available" pattern, with a configurable `maxAttempts` limit.
- `NicknameGenerator.DependencyInjection` now registers
  `IRetryableNicknameGenerator` alongside `INicknameGenerator`, sharing the same
  singleton instance.
- Golden-value tests for `attempt = 1` and `attempt = 2`, in addition to the
  `attempt = 0` set, so retry results are frozen too.

### Breaking

- `NicknameParts.ToString()` was removed. The override hardcoded `"_"` and
  ignored a custom `NicknameOptions.Separator`, producing an inconsistent string
  for non-default configurations. Use `INicknameGenerator.Generate(...)` or
  assemble the string from `Adjective`, `Noun`, `Suffix` yourself.

### Changed

- `Base62Encoder` and `EmbeddedWordListProvider` are now `internal` — they were
  `public` members of the `NicknameGenerator.Internal` namespace, which was
  misleading. They were never intended as public API.
- `NicknameOptions.SuffixBytes` and `NicknameOptions.Separator` XML docs were
  fixed (previously contained stray text outside `<summary>`).

### Fixed

- README collision probability table and rule-of-thumb corrected: the total
  space is `60 × 60 × 2^(8·SuffixBytes)`, not `60 × 60 × 62^(padded length)`.
  The previous numbers underestimated collision probability by up to 48×.
- README note about the first suffix character for `SuffixBytes = 5` corrected:
  the first character is uniformly distributed over 20 values (`'0'`..`'9'`,
  `'a'`..`'j'`), not biased toward `'0'` and `'1'`.

## [0.1.0-alpha] - 2026-10-21

### Added

- `NicknameGenerator` core library targeting `netstandard2.0`, `netstandard2.1`, `net8.0`.
- `HmacNicknameGenerator` — deterministic nickname generator based on HMAC-SHA256.
- `RandomNicknameGenerator` — non-deterministic generator for previews.
- `Base62Encoder` with fixed big-endian byte order for cross-platform determinism.
- Embedded English word list (v1): 60 adjectives × 60 nouns.
- `NicknameGenerator.DependencyInjection` — `IServiceCollection` extensions for
  ASP.NET Core, Worker Service, and console host with `IOptions<T>` validation.
- Golden-value tests to freeze deterministic output across releases.
- Distribution (χ²) and collision tests for statistical guarantees.

[Unreleased]: https://github.com/Humble-proger/NicknameGenerator/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/Humble-proger/NicknameGenerator/compare/v0.1.0-alpha...v0.2.0
[0.1.0-alpha]: https://github.com/Humble-proger/NicknameGenerator/releases/tag/v0.1.0-alpha