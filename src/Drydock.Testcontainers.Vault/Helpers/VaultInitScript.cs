namespace Drydock.Testcontainers.Vault.Helpers;

internal static class VaultInitScript
{
    private const string Shell = "/bin/sh";

    private const string CommandFlag = "-c";

    private const string StopOnFirstError = "set -e";

    private const string LineBreak = "\n";

    private const string DefaultCliPrefix = "vault ";

    private const string EmptyCommand = "An init command must not be empty.";

    private static readonly string[] CliPrefixes = { DefaultCliPrefix, "bao " };

    public static string[] Ensure(string[] commands, string parameterName)
    {
        if (commands is null)
            throw new ArgumentNullException(parameterName);

        if (Array.Exists(commands, string.IsNullOrWhiteSpace))
            throw new ArgumentException(EmptyCommand, parameterName);

        return commands;
    }

    public static IList<string> ToExecCommand(IEnumerable<string> commands)
    {
        return new[] { Shell, CommandFlag, Compose(commands) };
    }

    public static string Compose(IEnumerable<string> commands)
    {
        return string.Join(LineBreak, commands.Select(ToCliLine).Prepend(StopOnFirstError)) + LineBreak;
    }

    private static string ToCliLine(string command)
    {
        var line = command.Trim();

        return Array.Exists(CliPrefixes, prefix => line.StartsWith(prefix, StringComparison.Ordinal))
            ? line
            : DefaultCliPrefix + line;
    }
}
