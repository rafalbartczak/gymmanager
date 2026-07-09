using GymManager.Client;
using GymManager.Client.Services;
using GymManager.Client.Services.Auth;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;



namespace GymManager.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            var apiBase = builder.Configuration["Api:BaseUrl"]
                          ?? throw new Exception("Missing Api:BaseUrl");

            // 1) Domyœlny HttpClient dla zasobów klienta (Weather/FetchData itd.)
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });

            // 2) Tokeny + auth
            builder.Services.AddScoped<TokenStore>();
            builder.Services.AddScoped<AuthApiClient>(sp =>
            {
                var http = new HttpClient { BaseAddress = new Uri(apiBase) }; // go³y
                return new AuthApiClient(http);
            });

            builder.Services.AddScoped<AuthorizedHandler>();

            // 3) Autoryzowany klient do API (z handlerem)
            builder.Services.AddScoped<ApiHttpClient>(sp =>
            {
                var handler = sp.GetRequiredService<AuthorizedHandler>();
                handler.InnerHandler = new HttpClientHandler();

                var http = new HttpClient(handler) { BaseAddress = new Uri(apiBase) };
                return new ApiHttpClient(http);
            });

            await builder.Build().RunAsync();
        }
    }
}
