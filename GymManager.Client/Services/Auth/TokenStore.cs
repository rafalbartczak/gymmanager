using Microsoft.JSInterop;

namespace GymManager.Client.Services.Auth;

public class TokenStore
{
    private const string AccessKey = "gm_access";
    private const string RefreshKey = "gm_refresh";
    private readonly IJSRuntime _js;

    public TokenStore(IJSRuntime js) => _js = js;

    public Task SetTokensAsync(string accessToken, string refreshToken)
        => Task.WhenAll(
            _js.InvokeVoidAsync("authStorage.set", AccessKey, accessToken).AsTask(),
            _js.InvokeVoidAsync("authStorage.set", RefreshKey, refreshToken).AsTask()
        );

    public async Task<string?> GetAccessTokenAsync()
        => await _js.InvokeAsync<string?>("authStorage.get", AccessKey);

    public async Task<string?> GetRefreshTokenAsync()
        => await _js.InvokeAsync<string?>("authStorage.get", RefreshKey);

    public Task ClearAsync()
        => Task.WhenAll(
            _js.InvokeVoidAsync("authStorage.remove", AccessKey).AsTask(),
            _js.InvokeVoidAsync("authStorage.remove", RefreshKey).AsTask()
        );
}