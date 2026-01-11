using Newtonsoft.Json;
using Assignment_A1_01.Models;

namespace Assignment_A1_01.Services;

public class OpenWeatherService
{
    HttpClient _httpClient = new HttpClient();
    readonly string _apiKey = "5c1873370d6530313e703ee2ce959255"; // Replace with your OpenWeatherMap API key

    public async Task<Forecast> GetForecastAsync(double latitude, double longitude)
    {
        //https://openweathermap.org/current
        var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var uri = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&units=metric&lang={language}&appid={_apiKey}";

        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        
        //Convert Json to NewsResponse
        string content = await response.Content.ReadAsStringAsync();

        // Bara för att se rådatan. Ta bort innan färdig produktion
        // Console.WriteLine(content);

        Console.WriteLine(content);
        WeatherApiData wd = JsonConvert.DeserializeObject<WeatherApiData>(content);

        //Convert WeatherApiData to Forecast using Linq.
        //Your code
        //Hint: you will find 
        //City: wd.city.name
        //Daily forecast in wd.list, in an item in the list
        //      Date and time in Unix timestamp: dt 
        //      Temperature: main.temp
        //      WindSpeed: wind.speed
        //      Description:  first item in weather[].description
        //      Icon:  $"http://openweathermap.org/img/w/{wdle.weather.First().icon}.png"   //NOTE: Not necessary, only if you like to use an icon

        var forecast = new Forecast
        {
            City = wd.city.name,
            Items = wd.list
                    .Select(item => new ForecastItem
                    {
                        DateTime = UnixTimeStampToDateTime(item.dt),
                        Temperature = item.main.temp,
                        WindSpeed = item.wind.speed,
                        Description = item.weather.First().description,
                        Icon = $"http://openweathermap.org/img/w/{item.weather.First().icon}.png"
                    })
                    .ToList()
        };


        return forecast;
    }
    private DateTime UnixTimeStampToDateTime(double unixTimeStamp) => DateTime.UnixEpoch.AddSeconds(unixTimeStamp).ToLocalTime();
}

