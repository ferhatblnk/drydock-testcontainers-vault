# Contributing

Issues and pull requests are welcome.

## Before you start

- For a bug, open an issue with the image tag, the Testcontainers version and the smallest builder call that shows it.
- For a new option or a behaviour change, open an issue first. The module is small on purpose; an option is added when it removes work that most tests would otherwise repeat.

## Working on the code

```shell
dotnet test
```

Docker must be running. The suite starts real Vault and OpenBao containers and takes well under a minute.

- Every change in behaviour comes with a test. Recipes in the README have a test too.
- No comments in code. Public members carry XML documentation, because that is what package users see in their editor.
- Constants live in `Constants/`, helpers in `Helpers/`. Types stay small and do one thing.
- `dotnet format --verify-no-changes` must pass, and the build must stay free of warnings.
- Do not bind to more of the Testcontainers API than needed. The module is compiled once and has to keep working when the core is updated; `ContainerConfigurationSeed` exists for that reason.

## Checking another Testcontainers version

```shell
dotnet test "-p:TestcontainersTestVersion=4.*"
```
