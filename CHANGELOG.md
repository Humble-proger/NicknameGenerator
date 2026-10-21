# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/Humble-proger/NicknameGenerator/compare/v0.1.0-alpha...HEAD
[0.1.0-alpha]: https://github.com/Humble-proger/NicknameGenerator/releases/tag/v0.1.0-alpha