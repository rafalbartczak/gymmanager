using System.Net;
using System.Net.Http.Headers;

namespace GymManager.Client.Services.Auth;

/// <summary>
/// Przechwytuje żądania HTTP, dodaje token JWT do nagłówka Authorization
/// i automatycznie odświeża sesję przy odpowiedzi 401 (Unauthorized).
/// Flaga _isRefreshing zapobiega wielokrotnemu odświeżaniu przy równoległych żądaniach.
/// </summary>
public class AuthorizedHandler : DelegatingHandler
{
    private readonly TokenStore _tokens;
    private readonly AuthApiClient _auth;
    private bool _isRefreshing;

    public AuthorizedHandler(TokenStore tokens, AuthApiClient auth)
    {
        _tokens = tokens;
        _auth = auth;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var path = request.RequestUri?.AbsolutePath ?? "";
        var isAuthCall = path.Contains("/auth/", StringComparison.OrdinalIgnoreCase);

        var access = await _tokens.GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(access))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access);

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode != HttpStatusCode.Unauthorized || isAuthCall)
            return response;

        if (_isRefreshing) return response;
        _isRefreshing = true;

        try
        {
            var refresh = await _tokens.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refresh)) return response;

            var newTokens = await _auth.RefreshAsync(refresh);
            await _tokens.SetTokensAsync(newTokens.AccessToken, newTokens.RefreshToken);

            var retry = await CloneHttpRequestMessageAsync(request);
            retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.AccessToken);

            response.Dispose();
            return await base.SendAsync(retry, ct);
        }
        catch
        {
            await _tokens.ClearAsync();
            return response;
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}