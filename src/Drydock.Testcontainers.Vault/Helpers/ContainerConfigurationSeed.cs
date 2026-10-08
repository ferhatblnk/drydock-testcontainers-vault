namespace Drydock.Testcontainers.Vault.Helpers;

internal static class ContainerConfigurationSeed
{
    private const string UnsupportedCore = "This Testcontainers version has no container configuration that can be created with default values. Please report it at https://github.com/ferhatblnk/drydock-testcontainers-vault/issues.";

    private static readonly Lazy<IResourceConfiguration<CreateContainerParameters>> Seed = new(Create);

    public static IResourceConfiguration<CreateContainerParameters> Value => Seed.Value;

    private static IResourceConfiguration<CreateContainerParameters> Create()
    {
        var constructor = typeof(ContainerConfiguration)
            .GetConstructors()
            .OrderBy(candidate => candidate.GetParameters().Length)
            .FirstOrDefault(candidate => Array.TrueForAll(candidate.GetParameters(), parameter => parameter.IsOptional))
            ?? throw new NotSupportedException(UnsupportedCore);

        var arguments = Array.ConvertAll(constructor.GetParameters(), parameter => parameter.DefaultValue);

        return (ContainerConfiguration)constructor.Invoke(arguments);
    }
}
