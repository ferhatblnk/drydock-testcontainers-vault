using System.Text.RegularExpressions;

namespace Drydock.Testcontainers.Vault.Tests.Support;

internal static partial class TestImages
{
    private const string DockerfileName = "Dockerfile";

    private static readonly IReadOnlyDictionary<string, string> Stages = File
        .ReadLines(Path.Combine(AppContext.BaseDirectory, DockerfileName))
        .Select(line => Stage().Match(line))
        .Where(match => match.Success)
        .ToDictionary(match => match.Groups["stage"].Value, match => match.Groups["image"].Value);

    public static string Vault => Stages["vault"];

    public static string OpenBao => Stages["openbao"];

    [GeneratedRegex(@"^FROM\s+(?<image>\S+)\s+AS\s+(?<stage>\S+)\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex Stage();
}
