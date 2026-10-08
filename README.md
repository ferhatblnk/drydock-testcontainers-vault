# Drydock.Testcontainers.Vault

![Drydock](https://raw.githubusercontent.com/ferhatblnk/drydock-testcontainers-vault/main/assets/icon.png)

[![CI](https://github.com/ferhatblnk/drydock-testcontainers-vault/actions/workflows/ci.yml/badge.svg)](https://github.com/ferhatblnk/drydock-testcontainers-vault/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Drydock.Testcontainers.Vault.svg)](https://www.nuget.org/packages/Drydock.Testcontainers.Vault)

A Vault module for [Testcontainers for .NET](https://dotnet.testcontainers.org). Start a real [HashiCorp Vault](https://developer.hashicorp.com/vault) or [OpenBao](https://openbao.org) server from your .NET tests with one builder call. The container is ready when `StartAsync` returns, the root token is known, and the secrets, engines and policies your test needs are already there.

Testcontainers has a Vault module for Java, Go, Node.js and Python. This package brings the same to .NET. A dry dock is where a ship is checked before it sails, hence the name.

```csharp
await using var vault = new VaultBuilder("hashicorp/vault:2.1")
    .WithInitCommand("kv put secret/orders-api connection=Host=db")
    .Build();

await vault.StartAsync();

var client = new VaultClient(new VaultClientSettings(
    vault.GetBaseAddress(),
    new TokenAuthMethodInfo(vault.GetRootToken())));

var secret = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync("orders-api", mountPoint: "secret");
```

The example reads the secret with [VaultSharp](https://github.com/rajanadar/VaultSharp). The module itself depends only on Testcontainers, so any client or plain `HttpClient` works.

## Install

```shell
dotnet add package Drydock.Testcontainers.Vault
```

You need Docker and Testcontainers for .NET 4.14.0 or later. NuGet brings Testcontainers in for you.

## What it does for you

**Ready means ready.** `StartAsync` waits until Vault reports itself initialized, unsealed and active on `/v1/sys/health`. No sleeps, no retry loops in your tests.

**The root token is known.** It is `root` unless you call `WithRootToken`, and `GetRootToken()` returns it. A token Vault would reject, such as one containing a period, fails in the builder with a clear message. Without that check the container exits and the only hint is `invalid request` in its log.

**Init commands run in order and fail fast.** `WithInitCommand` takes Vault CLI commands and runs them once the server is ready, in one shell call. If one fails, `StartAsync` throws `VaultInitCommandException` with the exit code and the CLI's error, and the commands after it do not run. Your test fails at the cause, not three assertions later.

**The CLI inside the container is authenticated.** `VAULT_ADDR` and `VAULT_TOKEN` are set, so `vault.ExecAsync(["vault", "read", ...])` works without extra flags.

**Vault and OpenBao use the same builder.** Pass `openbao/openbao:2.7` instead of the Vault image and nothing else changes.

**No extra Linux capability is requested.** Dev mode never locks memory, so the module does not ask Docker for `IPC_LOCK`. Some CI services refuse containers that ask for it ([testcontainers-java#6623](https://github.com/testcontainers/testcontainers-java/issues/6623)).

**It keeps working when Testcontainers is updated.** The section "Staying compatible with Testcontainers" below explains how.

## API

| Member | What it does |
| --- | --- |
| `new VaultBuilder(string image)` | Creates the builder. The image and tag are yours to pin, for example `hashicorp/vault:2.1`. |
| `WithRootToken(string)` | Sets the root token. Default: `VaultBuilder.DefaultRootToken` (`root`). |
| `WithInitCommand(params string[])` | Adds CLI commands that run in order after the server is ready. The leading `vault` is optional. |
| `VaultContainer.GetBaseAddress()` | The address from the test host, for example `http://127.0.0.1:32768/`. |
| `VaultContainer.GetRootToken()` | The root token. |
| `VaultContainer.GetConnectionString()` | Same value as `GetBaseAddress()`. |
| `VaultBuilder.VaultPort` | The port inside the container, `8200`. |
| `VaultInitCommandException` | Thrown by `StartAsync` when an init command fails. Has `ExitCode`, `Stdout` and `Stderr`. |

`VaultBuilder` is a regular Testcontainers builder, so `WithNetwork`, `WithEnvironment`, `WithResourceMapping` and the rest are available too.

## Recipes

Every recipe below has a test in this repository.

### Use it in an xUnit test class

```csharp
public sealed class OrdersApiTest : IAsyncLifetime
{
    private readonly VaultContainer _vault = new VaultBuilder("hashicorp/vault:2.1")
        .WithInitCommand("kv put secret/orders-api connection=Host=db")
        .Build();

    public async ValueTask InitializeAsync() => await _vault.StartAsync();

    public ValueTask DisposeAsync() => _vault.DisposeAsync();
}
```

This is xUnit v3. In xUnit v2 the two methods return `Task`.

### Enable engines and create keys

Commands run in the order you add them, across several `WithInitCommand` calls as well.

```csharp
var vault = new VaultBuilder("hashicorp/vault:2.1")
    .WithInitCommand("secrets enable transit", "write -f transit/keys/orders")
    .WithInitCommand("secrets enable -path=legacy -version=1 kv", "kv put legacy/db username=demo")
    .Build();
```

### Write a policy

Each command is a line of shell, so a here-document works.

```csharp
const string policy = """
    policy write reader - <<EOF
    path "secret/data/*" {
      capabilities = ["read"]
    }
    EOF
    """;

var vault = new VaultBuilder("hashicorp/vault:2.1")
    .WithInitCommand(policy)
    .Build();
```

### Log in with AppRole

Create the role with init commands, then read its credentials through the CLI.

```csharp
await using var vault = new VaultBuilder("hashicorp/vault:2.1")
    .WithInitCommand("auth enable approle", "write auth/approle/role/orders-api token_policies=default")
    .Build();

await vault.StartAsync();

var roleId = await vault.ExecAsync(["vault", "read", "-field=role_id", "auth/approle/role/orders-api/role-id"]);
var secretId = await vault.ExecAsync(["vault", "write", "-f", "-field=secret_id", "auth/approle/role/orders-api/secret-id"]);
```

`roleId.Stdout` and `secretId.Stdout` hold the values.

### Reach Vault from another container

Put both containers on one network and give Vault an alias. The other container uses the alias and the internal port.

```csharp
await using var network = new NetworkBuilder().Build();

await using var vault = new VaultBuilder("hashicorp/vault:2.1")
    .WithNetwork(network)
    .WithNetworkAliases("vault")
    .Build();

var addressInsideTheNetwork = $"http://vault:{VaultBuilder.VaultPort}";
```

### Handle a failing init command

```csharp
try
{
    await vault.StartAsync();
}
catch (VaultInitCommandException exception)
{
    Console.WriteLine(exception.ExitCode);
    Console.WriteLine(exception.Stderr);
}
```

### Run OpenBao

```csharp
var openBao = new VaultBuilder("openbao/openbao:2.7").Build();
```

## Tested with

| | Versions |
| --- | --- |
| HashiCorp Vault | 1.13, 1.19, 2.1 |
| OpenBao | 2.4, 2.7 |
| Testcontainers for .NET | 4.14.0 and the newest 4.x |
| .NET | 8, 9, 10 (the library targets .NET Standard 2.0) |

The test suite starts every image above on each run.

## Staying compatible with Testcontainers

Testcontainers modules are normally compiled against one exact shape of an internal constructor. When that constructor gains a parameter, every module built before the change throws `MissingMethodException` on the first `new XyzBuilder(...)`. It happened in Testcontainers 4.7, 4.8 and 4.10, and the official modules handle it by always releasing together with the core.

A package that releases on its own cannot do that, so this module does not bind to that constructor. In practice:

- Updating Testcontainers does not require a new release of this package.
- CI runs the whole suite against the lowest supported version and against the newest 4.x release, on every change and once a week.

Two things to know about versions:

- The package requires Testcontainers 4.14.0 or later, because every earlier 4.x version depends on an SSH.NET release with known vulnerabilities and NuGet would warn about it.
- If your project still uses official modules older than 4.10, installing this package raises the shared Testcontainers core to 4.14, and those old modules stop working with it. Update them to the same version as the core, as the Testcontainers maintainers recommend.

## Limits

- **Dev mode only.** The server keeps everything in memory, runs as a single node and speaks plain HTTP. That is what makes it fast and disposable, and it is why the module is for tests only.
- **Init commands run after every start.** With `WithReuse(true)` the container survives between runs, so use commands that can run twice, or expect the second run to fail on something like `secrets enable`.
- **`WithWaitStrategy` replaces the health check**, as with any Testcontainers module.
- **Linux containers only.**

## Building and testing

```shell
dotnet test
```

Docker must be running. The images under test are listed in [`tests/Drydock.Testcontainers.Vault.Tests/Dockerfile`](https://github.com/ferhatblnk/drydock-testcontainers-vault/blob/main/tests/Drydock.Testcontainers.Vault.Tests/Dockerfile) so that they can be updated automatically.

To run the suite against another Testcontainers version:

```shell
dotnet test -p:TestcontainersTestVersion=4.*
```

## Background

The story behind the module, and what building it taught me about Testcontainers, is in this article: [Your code reads secrets from Vault. What do you test it against?](https://medium.com/@ferhatblnk/your-code-reads-secrets-from-vault-what-do-you-test-it-against-349fbe78c2e0)

## Contributing

Issues and pull requests are welcome. For anything larger than a fix, please open an issue first so we can agree on the approach. [CONTRIBUTING.md](https://github.com/ferhatblnk/drydock-testcontainers-vault/blob/main/CONTRIBUTING.md) has the short list of rules.

This module was developed with an AI coding assistant (Claude). The behaviour described in this README is covered by tests that run against real Vault and OpenBao containers. Contributions written with AI tools are welcome on the same terms: they come with tests.

## Trademarks and third-party software

Drydock.Testcontainers.Vault is an independent community project. It is not affiliated with, sponsored by or endorsed by HashiCorp, IBM, the OpenBao project, Docker or the maintainers of Testcontainers.

HashiCorp and Vault are trademarks of HashiCorp, Inc. Docker, Testcontainers and OpenBao are trademarks of their respective owners. These names appear here only to say what the module works with. The project uses none of their logos; the icon is an original drawing.

The package contains only its own code:

- It does not contain or redistribute Vault or OpenBao. It starts the image you name, and that image comes under its own license: the Business Source License for HashiCorp Vault, MPL-2.0 for OpenBao.
- [Testcontainers for .NET](https://github.com/testcontainers/testcontainers-dotnet) (MIT) is a NuGet dependency, not a bundled copy. The builder follows that project's module conventions.
- VaultSharp and xUnit are used by the tests only and are not part of the package.

## License

[MIT](https://github.com/ferhatblnk/drydock-testcontainers-vault/blob/main/LICENSE)
