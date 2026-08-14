using System.Text.Json;

namespace Ecommerce.API.Configuration;

public static class VaultConfiguration
{
    public static void AddVaultSecrets(this ConfigurationManager configuration)
    {
        if (!configuration.GetValue("Vault:Enabled", false)) return;
        var address = configuration["Vault:Address"] ?? throw new InvalidOperationException("Vault:Address is required.");
        var token = configuration["Vault:Token"] ?? throw new InvalidOperationException("Vault:Token is required.");
        var path = configuration["Vault:SecretPath"] ?? "secret/data/ecommerce";
        using var client = new HttpClient { BaseAddress = new Uri(address.TrimEnd('/') + "/") };
        client.DefaultRequestHeaders.Add("X-Vault-Token", token);
        using var response = client.GetAsync($"v1/{path}").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(response.Content.ReadAsStream());
        var secrets = document.RootElement.GetProperty("data").GetProperty("data");
        var values = secrets.EnumerateObject().ToDictionary(
            item => item.Name.Replace("__", ":", StringComparison.Ordinal),
            item => (string?)item.Value.GetString());
        configuration.AddInMemoryCollection(values);
    }
}
