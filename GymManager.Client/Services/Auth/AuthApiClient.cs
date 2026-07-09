using System.Net.Http.Json;
using GymManager.Client.Contracts;

namespace GymManager.Client.Services.Auth;

public class AuthApiClient
{
    private readonly HttpClient _http;

    public AuthApiClient(HttpClient http) => _http = http;

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var res = await _http.PostAsJsonAsync("auth/login", new LoginRequest(email, password));
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
    {
        var res = await _http.PostAsJsonAsync("auth/register", req);
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var res = await _http.PostAsJsonAsync("auth/refresh", new RefreshRequest(refreshToken));
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var res = await _http.PostAsJsonAsync("auth/logout", new LogoutRequest(refreshToken));
        // logout ma prawo zwrócić 200 nawet jak token nie istnieje
        res.EnsureSuccessStatusCode();
    }
}