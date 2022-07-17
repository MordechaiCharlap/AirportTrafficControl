namespace AirportTrafficControlTower.Client.Helper
{
    public class AirportApi
    {
        public HttpClient Initial()
        {
            var Client = new HttpClient();
            Client.BaseAddress = new Uri("https://localhost:7294/");
            return Client;
        }
    }
}
