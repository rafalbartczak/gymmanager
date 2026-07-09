using System.Security.Cryptography;

namespace GymManager.Api.Services.Security;

/// <summary>
/// Generowanie kryptograficznie bezpiecznych tokenów odświeżania (32 bajty entropii)
/// i ich hashowanie SHA-256 przed zapisem do bazy danych.
/// </summary>
public class RefreshTokenService
{
    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64UrlEncode(bytes);
    }

    public byte[] ComputeTokenHash(string refreshToken)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(refreshToken);
        return SHA256.HashData(bytes);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}