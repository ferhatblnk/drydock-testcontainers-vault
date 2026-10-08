namespace Drydock.Testcontainers.Vault.Helpers;

internal static class VaultTokenRules
{
    private const char Period = '.';

    private const string Empty = "The root token must not be empty.";

    private const string ContainsPeriod = "The root token must not contain a period ('.'). Vault and OpenBao reject such a token in dev mode with 'invalid request' and the container exits.";

    public static string Ensure(string rootToken, string parameterName)
    {
        if (rootToken is null)
            throw new ArgumentNullException(parameterName);

        if (string.IsNullOrWhiteSpace(rootToken))
            throw new ArgumentException(Empty, parameterName);

        if (rootToken.IndexOf(Period) >= 0)
            throw new ArgumentException(ContainsPeriod, parameterName);

        return rootToken;
    }
}
