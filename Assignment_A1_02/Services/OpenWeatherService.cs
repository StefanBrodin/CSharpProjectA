using Assignment_A1_02.Models;
using Newtonsoft.Json;

namespace Assignment_A1_02.Services;
public class OpenWeatherService
{
    readonly HttpClient _httpClient = new HttpClient();
    readonly string _apiKey = "5c1873370d6530313e703ee2ce959255"; // Replace with your OpenWeatherMap API key

    // Event declaration
    public event EventHandler<string> WeatherForecastAvailable;
    protected virtual void OnWeatherForecastAvailable (string message)
    {
        WeatherForecastAvailable?.Invoke(this, message);
    }

    // Forecast by City
    public async Task<Forecast> GetForecastAsync(string city)
    {
        //https://openweathermap.org/current
        var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var uri = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&units=metric&lang={language}&appid={_apiKey}";

        Forecast forecast = await ReadWebApiAsync(uri);

        // Event code here to fire the event
        OnWeatherForecastAvailable($"New weather forecast for {city} available");
        return forecast;
    }

    // Forecast by GeoLocation
    public async Task<Forecast> GetForecastAsync(double latitude, double longitude)
    {
        //https://openweathermap.org/current
        var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var uri = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&units=metric&lang={language}&appid={_apiKey}";

        Forecast forecast = await ReadWebApiAsync(uri);

        // Event code here to fire the event
        OnWeatherForecastAvailable($"New weather forecast for ({latitude}, {longitude}) available");
        return forecast;
    }

    private async Task<Forecast> ReadWebApiAsync(string uri)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        response.EnsureSuccessStatusCode();

        // Convert Json to NewsResponse
        string content = await response.Content.ReadAsStringAsync();
        WeatherApiData wd = JsonConvert.DeserializeObject<WeatherApiData>(content);

        // Create the forecast object of type Forecast based on WeatherApiData.
        // Now a bit more fail-safe by better handling of potentially missing data/null
        var forecast = new Forecast
        {
            City = wd.city.name,
            Items = wd.list
                    .Select(item => new ForecastItem
                    {
                        DateTime = UnixTimeStampToDateTime(item.dt), // Can never be NULL after serialization, at worst it will be 0 which will mean 1970-01-01, an obvious error!
                        Temperature = item.main?.temp ?? double.NaN,
                        WindSpeed = item.wind?.speed ?? double.NaN,
                        Description = item.weather.FirstOrDefault()?.description ?? "No Description!",
                        Icon = item.weather.FirstOrDefault()?.icon is string icon
                            ? $"http://openweathermap.org/img/w/{icon}.png"
                            : null
                    })
                    .ToList()
        };

        return forecast;
    }

    private DateTime UnixTimeStampToDateTime(double unixTimeStamp) => DateTime.UnixEpoch.AddSeconds(unixTimeStamp).ToLocalTime();
}

