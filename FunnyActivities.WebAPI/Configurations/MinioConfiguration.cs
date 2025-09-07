using System;

namespace FunnyActivities.WebAPI.Configurations;

public record MinioConfiguration
{
    public string Endpoint { get; init; }
    public string AccessKey { get; init; }
    public string SecretKey { get; init; }
    public bool UseSSL { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
            throw new InvalidOperationException("MinIO configuration error: Endpoint is required and cannot be empty or whitespace.");

        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out _))
            throw new InvalidOperationException("MinIO configuration error: Endpoint must be a valid absolute URI.");

        if (string.IsNullOrWhiteSpace(AccessKey))
            throw new InvalidOperationException("MinIO configuration error: AccessKey is required and cannot be empty or whitespace.");

        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new InvalidOperationException("MinIO configuration error: SecretKey is required and cannot be empty or whitespace.");

        if (AccessKey.Length < 3)
            throw new InvalidOperationException("MinIO configuration error: AccessKey must be at least 3 characters long.");

        if (SecretKey.Length < 8)
            throw new InvalidOperationException("MinIO configuration error: SecretKey must be at least 8 characters long.");
    }
}