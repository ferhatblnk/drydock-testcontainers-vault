using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Drydock.Testcontainers.Vault.Tests.Support;

internal sealed class VaultApi : IDisposable
{
    private const string TokenHeader = "X-Vault-Token";

    private readonly HttpClient _client;

    public VaultApi(string baseAddress, string token)
    {
        _client = new HttpClient { BaseAddress = new Uri(baseAddress) };
        _client.DefaultRequestHeaders.Add(TokenHeader, token);
    }

    public static VaultApi For(VaultContainer container)
    {
        return new VaultApi(container.GetBaseAddress(), container.GetRootToken());
    }

    public async Task<HttpStatusCode> GetStatusAsync(string path, CancellationToken ct)
    {
        using var response = await _client.GetAsync(path, ct);
        return response.StatusCode;
    }

    public async Task<JsonElement> GetAsync(string path, CancellationToken ct)
    {
        using var response = await _client.GetAsync(path, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>(ct);
    }

    public async Task<JsonElement> PostAsync(string path, object body, CancellationToken ct)
    {
        using var response = await _client.PostAsJsonAsync(path, body, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>(ct);
    }

    public async Task WriteSecretAsync(string name, string key, string value, CancellationToken ct)
    {
        _ = await PostAsync($"v1/secret/data/{name}", new { data = new Dictionary<string, string> { [key] = value } }, ct);
    }

    public async Task<string?> ReadSecretAsync(string name, string key, CancellationToken ct)
    {
        var secret = await GetAsync($"v1/secret/data/{name}", ct);
        return secret.GetProperty("data").GetProperty("data").GetProperty(key).GetString();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
