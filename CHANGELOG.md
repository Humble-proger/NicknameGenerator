# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

- `RetryableNicknameGeneratorExtensions.GenerateWithRetryAsync` — async
  version of the retry helper for scenarios where the availability check
  performs I/O (database, HTTP). The generator itself remains synchronous
  (CPU-bound); only the `isAvailableAsync` callback is awaited.

## [0.3.0] - 2026-10-23

### Added

- SourceLink support: debugging into `HmacNicknameGenerator` or
  `RandomNicknameGenerator` from a consuming project now navigates to the exact
  source commit on GitHub. Enabled via `<PublishRepositoryUrl>true</PublishRepositoryUrl>`
  and `<EmbedUntrackedSources>true</EmbedUntrackedSources>` in `.csproj`; CI
  builds with `/p:ContinuousIntegrationBuild=true` for deterministic paths.
- `Microsoft.CodeAnalysis.PublicApiAnalyzers` — the public API surface is now
  explicitly tracked in `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`.
  Any accidental change to public API (added, removed, or renamed member) will
  fail the build, preventing unintended breaks before `1.0.0`.
  Pinned to `3.3.4`: `5.x` requires Roslyn `4.12.0.0`, while the current .NET 8
  SDK ships with `4.11.0.0` (fails with `CS9057`). Will be upgraded when the
  SDK is.
- BenchmarkDotNet suite for `HmacNicknameGenerator.Generate` (attempt 0 and 1),
  `GenerateParts`, and `RandomNicknameGenerator.Generate`. Baseline results in
  the README (416–528 B per call, <1.5 µs on a 2019 mobile CPU).
- Samples:
  - `samples/Console` — minimal quick-start.
  - `samples/AspNetCore` — Minimal API + DI + `IRetryableNicknameGenerator`
    endpoint.
  - `samples/Unity` — README + `Example.cs` (DLL deployment, `link.xml` for
    IL2CPP).
- Integration tests (`tests/NicknameGenerator.Integration.Tests`) using
  `WebApplicationFactory<Program>` — verifies DI + options binding + HTTP
  contract end-to-end.

### Changed

- Minor style cleanups: simplified local initialization and removed redundant
  duplicate code paths.

### Fixed

- `DistributionTests`: corrected chi-square critical values for `α = 0.01`.
  - `df = 59`: `88.4` → `87.1657`
  - `df = 61`: `95.0` (also inconsistent with the message) → `89.5913`
  The previous higher thresholds made the uniformity tests blind to moderate
  distribution anomalies — a real regression would slip through if χ² landed
  between the correct and inflated values.

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
[0.3.0]: https://github.com/Humble-proger/NicknameGenerator/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/Humble-proger/NicknameGenerator/compare/v0.1.0-alpha...v0.2.0
[0.1.0-alpha]: https://github.com/Humble-proger/NicknameGenerator/releases/tag/v0.1.0-alpha