# Changelog

All notable changes to this project are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.1] - 2026-10-08

### Added

- Package icon.

## [0.1.0] - 2026-10-08

### Added

- `VaultBuilder` and `VaultContainer` for HashiCorp Vault and OpenBao in dev mode.
- Readiness check on `/v1/sys/health`.
- `WithRootToken` with validation of tokens the server would reject, and `GetRootToken`.
- `WithInitCommand`: ordered CLI commands that run after the server is ready and fail fast with `VaultInitCommandException`.
- `GetBaseAddress` and `GetConnectionString`.
- An authenticated CLI inside the container through `VAULT_ADDR` and `VAULT_TOKEN`.
