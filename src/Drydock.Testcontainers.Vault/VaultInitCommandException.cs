namespace Drydock.Testcontainers.Vault;

/// <summary>
/// Thrown by <see cref="VaultContainer.StartAsync" /> when an init command exits with a non-zero code.
/// </summary>
public sealed class VaultInitCommandException : Exception
{
    private const string MessageFormat = "The Vault init commands failed with exit code {0}: {1}";

    private const string UnknownExitCode = "unknown";

    internal VaultInitCommandException(ExecResult result)
        : base(Describe(result))
    {
        ExitCode = result.ExitCode;
        Stdout = result.Stdout;
        Stderr = result.Stderr;
    }

    /// <summary>
    /// Gets the exit code of the shell that ran the init commands.
    /// </summary>
    public long? ExitCode { get; }

    /// <summary>
    /// Gets what the commands wrote to standard output before the failure.
    /// </summary>
    public string Stdout { get; }

    /// <summary>
    /// Gets what the failing command wrote to standard error.
    /// </summary>
    public string Stderr { get; }

    private static string Describe(ExecResult result)
    {
        var exitCode = result.ExitCode?.ToString(CultureInfo.InvariantCulture) ?? UnknownExitCode;
        var output = string.IsNullOrWhiteSpace(result.Stderr) ? result.Stdout : result.Stderr;

        return string.Format(CultureInfo.InvariantCulture, MessageFormat, exitCode, output?.Trim());
    }
}
