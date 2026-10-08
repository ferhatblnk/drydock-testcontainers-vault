namespace Drydock.Testcontainers.Vault;

/// <summary>
/// Builds a <see cref="VaultContainer" /> that runs HashiCorp Vault or OpenBao in dev mode.
/// </summary>
public sealed class VaultBuilder : ContainerBuilder<VaultBuilder, VaultContainer, VaultConfiguration>
{
    /// <summary>
    /// The port the server listens on inside the container.
    /// </summary>
    public const ushort VaultPort = 8200;

    /// <summary>
    /// The root token used when <see cref="WithRootToken" /> is not called.
    /// </summary>
    public const string DefaultRootToken = "root";

    /// <summary>
    /// Initializes a new instance of the <see cref="VaultBuilder" /> class.
    /// </summary>
    /// <param name="image">
    /// The full image name including the tag, for example <c>hashicorp/vault:2.1</c> or <c>openbao/openbao:2.7</c>.
    /// </param>
    public VaultBuilder(string image)
        : this(new VaultConfiguration())
    {
        DockerResourceConfiguration = Init().WithImage(image).DockerResourceConfiguration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VaultBuilder" /> class.
    /// </summary>
    /// <param name="image">The image to run, for example one built from a Dockerfile.</param>
    public VaultBuilder(IImage image)
        : this(new VaultConfiguration())
    {
        DockerResourceConfiguration = Init().WithImage(image).DockerResourceConfiguration;
    }

    private VaultBuilder(VaultConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override VaultConfiguration DockerResourceConfiguration { get; }

    /// <summary>
    /// Sets the root token of the dev server. The same token authenticates the CLI inside the container.
    /// </summary>
    /// <param name="rootToken">The root token. It must not be empty or contain a period.</param>
    /// <returns>A configured instance of <see cref="VaultBuilder" />.</returns>
    /// <exception cref="ArgumentException">The token is empty or contains a period, which Vault and OpenBao reject.</exception>
    public VaultBuilder WithRootToken(string rootToken)
    {
        var token = VaultTokenRules.Ensure(rootToken, nameof(rootToken));

        return Merge(DockerResourceConfiguration, new VaultConfiguration(rootToken: token))
            .WithEnvironment(VaultEnvironment.VaultDevRootToken, token)
            .WithEnvironment(VaultEnvironment.OpenBaoDevRootToken, token)
            .WithEnvironment(VaultEnvironment.Token, token);
    }

    /// <summary>
    /// Adds CLI commands that run in order once the server is ready, for example
    /// <c>secrets enable transit</c> or <c>kv put secret/app user=demo</c>.
    /// The leading <c>vault</c> is optional. The first failing command stops the start with a
    /// <see cref="VaultInitCommandException" /> and the commands after it do not run.
    /// </summary>
    /// <param name="commands">The commands, each interpreted by the shell inside the container.</param>
    /// <returns>A configured instance of <see cref="VaultBuilder" />.</returns>
    /// <exception cref="ArgumentException">A command is empty.</exception>
    public VaultBuilder WithInitCommand(params string[] commands)
    {
        return Merge(DockerResourceConfiguration, new VaultConfiguration(initCommands: VaultInitScript.Ensure(commands, nameof(commands))));
    }

    /// <inheritdoc />
    public override VaultContainer Build()
    {
        Validate();
        return new VaultContainer(DockerResourceConfiguration);
    }

    /// <inheritdoc />
    protected override VaultBuilder Init()
    {
        return base.Init()
            .WithPortBinding(VaultPort, true)
            .WithRootToken(DefaultRootToken)
            .WithEnvironment(VaultEnvironment.Address, VaultServer.LoopbackAddress)
            .WithConnectionStringProvider(new VaultConnectionStringProvider())
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request =>
                request.ForPath(VaultServer.HealthPath).ForPort(VaultPort)));
    }

    /// <inheritdoc />
    protected override VaultBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new VaultConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override VaultBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new VaultConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override VaultBuilder Merge(VaultConfiguration oldValue, VaultConfiguration newValue)
    {
        return new VaultBuilder(new VaultConfiguration(oldValue, newValue));
    }
}
