using System.Text.Json;

namespace GymManager.Client.Services.Auth;

/// <summary>
/// Dekodowanie payloadu JWT po stronie klienta (bez weryfikacji podpisu).
/// Służy do odczytu roli i identyfikatora użytkownika z tokena.
/// </summary>
public static class JwtHelper
{
    public static string? GetRole(string jwt)
    {
        var payload = DecodePayload(jwt);
        if (payload is null) return null;

        if (payload.TryGetValue("role", out var roleEl) && roleEl.ValueKind == JsonValueKind.String)
            return roleEl.GetString();

        var roleClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        if (payload.TryGetValue(roleClaim, out var roleEl2) && roleEl2.ValueKind == JsonValueKind.String)
            return roleEl2.GetString();

        return null;
    }

    public static string? GetUserId(string jwt)
    {
        var payload = DecodePayload(jwt);
        if (payload is null) return null;

        if (payload.TryGetValue("sub", out var subEl) && subEl.ValueKind == JsonValueKind.String)
            return subEl.GetString();

        if (payload.TryGetValue("nameid", out var nameidEl) && nameidEl.ValueKind == JsonValueKind.String)
            return nameidEl.GetString();

        var nameidClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        if (payload.TryGetValue(nameidClaim, out var nameidEl2) && nameidEl2.ValueKind == JsonValueKind.String)
            return nameidEl2.GetString();

        return null;
    }

    public static string? GetEmail(string jwt)
    {
        var payload = DecodePayload(jwt);
        if (payload is null) return null;

        if (payload.TryGetValue("email", out var emailEl) && emailEl.ValueKind == JsonValueKind.String)
            return emailEl.GetString();

        return null;
    }

    private static Dictionary<string, JsonElement>? DecodePayload(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3) return null;

        var payload = parts[1];
        var jsonBytes = Base64UrlDecode(payload);
        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);
    }

    private static byte[] Base64UrlDecode(string input)
    {
        input = input.Replace('-', '+').Replace('_', '/');
        switch (input.Length % 4)
        {
            case 2: input += "=="; break;
            case 3: input += "="; break;
        }
        return Convert.FromBase64String(input);
    }
}