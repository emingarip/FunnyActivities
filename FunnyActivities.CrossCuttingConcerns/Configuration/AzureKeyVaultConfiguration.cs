using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace FunnyActivities.CrossCuttingConcerns.Configuration;

public static class AzureKeyVaultConfiguration
{
    public static IConfigurationBuilder AddAzureKeyVault(this IConfigurationBuilder builder, string keyVaultUri)
    {
        var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
        builder.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());

        return builder;
    }
}