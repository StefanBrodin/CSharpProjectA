using System.Collections.Concurrent;
using Newtonsoft.Json;

using Assignment_A1_03.Models;

namespace Assignment_A1_03.Services;

public class OpenWeatherService
{
    readonly HttpClient _httpClient = new HttpClient();
    readonly ConcurrentDictionary<(double, double, string), Forecast> _cachedGeoForecasts = new ConcurrentDictionary<(double, double, string), Forecast>();
    readonly ConcurrentDictionary<(string, string), Forecast> _cachedCityForecasts = new ConcurrentDictionary<(string, string), Forecast>();

    // Your API Key
    readonly string apiKey = "5c1873370d6530313e703ee2ce959255"; // Replace with your OpenWeatherMap API key

    // Event declaration
    public event EventHandler<string> WeatherForecastAvailable;
    protected virtual void OnWeatherForecastAvailable (string message)
    {
        WeatherForecastAvailable?.Invoke(this, message);
    }

    // Forecast by City
    public async Task<Forecast> GetForecastAsync(string city)
    {
        // Normalize and trim the city name to make it common and comparable
        string cityKey = city.Trim().ToLower();

        // Create a key-Tuple based on city and time rounded to hours and minutes
        string timeKey = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        var cacheKey = (cityKey, timeKey);

        // part of cache code here to check if forecast in Cache
        // generate an event that shows forecast was from cache
        if (_cachedCityForecasts.TryGetValue(cacheKey, out var cachedForecast))
        {
            // We ahve a cached forecast for this minute, use this and send event
            OnWeatherForecastAvailable($"Cached weather forecast for {city} available");
            return cachedForecast;
        }

        // If we get to here there is no valid forecast in cache. Fetch a new forecast and save into cache.

        //https://openweathermap.org/current
        var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var uri = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&units=metric&lang={language}&appid={apiKey}";

        Forecast forecast = await ReadWebApiAsync(uri);

        //part of event and cache code here
        //generate an event with different message if cached data

        // Save new forecast to cache using the same key from above
        _cachedCityForecasts[cacheKey] = forecast;

        // Send event about having fetched new data
        OnWeatherForecastAvailable($"New weather forecast for {city} available");

        return forecast;

    }

    // Forecast by GeoLocation
    public async Task<Forecast> GetForecastAsync(double latitude, double longitude)
    {
        // Create a key-Tuple based on latitude, longitude and time rounded to hours and minutes
        string timeKey = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        var cacheKey = (latitude, longitude, timeKey);

        // part of cache code here to check if forecast in Cache
        // generate an event that shows forecast was from cache
        if (_cachedGeoForecasts.TryGetValue(cacheKey, out var cachedForecast))
        {
            // We ahve a cached forecast for this minute, use this and send event
            OnWeatherForecastAvailable($"Cached weather forecast for ({latitude}, {longitude}) available");
            return cachedForecast;
        }

        // If we get to here there is no valid forecast in cache. Fetch a new forecast and save into cache.

        //https://openweathermap.org/current
        var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var uri = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&units=metric&lang={language}&appid={apiKey}";

        Forecast forecast = await ReadWebApiAsync(uri);

        // part of event and cache code here
        // generate an event with different message if cached data

        // Save new forecast to cache using the same key from above
        _cachedGeoForecasts[cacheKey] = forecast;

        // Send event about having fetched new data
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
            City = wd.city?.name,
            Items = wd.list?
                    .Select(item => new ForecastItem
                    {
                        DateTime = UnixTimeStampToDateTime(item.dt),
                        Temperature = item.main?.temp,
                        WindSpeed = item.wind?.speed,
                        Description = item.weather.FirstOrDefault()?.description,
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

