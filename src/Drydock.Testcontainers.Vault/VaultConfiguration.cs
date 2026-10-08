namespace Drydock.Testcontainers.Vault;

/// <summary>
/// The immutable configuration of a <see cref="VaultContainer" />.
/// </summary>
public sealed class VaultConfiguration : ContainerConfiguration
{
    internal VaultConfiguration(string? rootToken = null, IEnumerable<string>? initCommands = null)
        : base(ContainerConfigurationSeed.Value)
    {
        RootToken = rootToken;
        InitCommands = initCommands?.ToArray() ?? Array.Empty<string>();
    }

    internal VaultConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
        InitCommands = Array.Empty<string>();
    }

    internal VaultConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        InitCommands = Array.Empty<string>();
    }

    internal VaultConfiguration(VaultConfiguration oldValue, VaultConfiguration newValue)
        : base(oldValue, newValue)
    {
        RootToken = newValue.RootToken ?? oldValue.RootToken;
        InitCommands = Combine(oldValue.InitCommands, newValue.InitCommands);
    }

    /// <summary>
    /// Gets the root token of the dev server.
    /// </summary>
    public string? RootToken { get; }

    /// <summary>
    /// Gets the init commands in the order they run.
    /// </summary>
    public IReadOnlyList<string> InitCommands { get; }

    private static IReadOnlyList<string> Combine(IReadOnlyList<string> oldValue, IReadOnlyList<string> newValue)
    {
        if (newValue.Count == 0)
            return oldValue;

        if (oldValue.Count == 0)
            return newValue;

        return oldValue.Concat(newValue).ToArray();
    }
}
