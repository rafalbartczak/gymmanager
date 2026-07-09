namespace GymManager.Client.Services
{
    public class ApiHttpClient
    {
        public HttpClient Http { get; }
        public ApiHttpClient(HttpClient http) => Http = http;
    }
}
