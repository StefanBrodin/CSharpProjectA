using Assignment_A1_01.Models;
using Assignment_A1_01.Services;

namespace Assignment_A1_01;

class Program
{
    static async Task Main(string[] args)
    {
        double latitude = 59.5086798659495;
        double longitude = 18.2654625932976;

        Forecast forecast = await new OpenWeatherService().GetForecastAsync(latitude, longitude);

        //Your Code to present each forecast item in a grouped list

        string forecastTitle = $"Väderprognos för {forecast.City}";
        Console.WriteLine(forecastTitle);
        Console.WriteLine(new string('-', forecastTitle.Length));

        var groupedByDay = forecast.Items
            .GroupBy(f => f.DateTime.Date)
            .OrderBy(g => g.Key);

        foreach (var groupOfDays in groupedByDay)
        {
            string forecastDate = $"{groupOfDays.Key:dddd dd MMMM yyyy}".FirstCharToUpper();

            Console.WriteLine($"{forecastDate}");

            // Now the printout can handle potential missing values
            foreach (var item in groupOfDays)
            {
                string timeStr = item.DateTime.ToString("HH:mm");
                string tempStr = item.Temperature?.ToString("F1") ?? "(inget värde)";
                string windStr = item.WindSpeed?.ToString("F1") ?? "(inget värde)";
                string descStr = item.Description.FirstCharToUpper() ?? "(inget värde)";

                string capitalized = descStr.FirstCharToUpper();

                Console.WriteLine($"   - {timeStr}: " +
                                  $"{descStr}, " +
                                  $"temperatur: {tempStr} °C, " +
                                  $"vind: {windStr} m/s.");
            }

            Console.WriteLine();
        }
    }

}

public static class MyExtensions
{
    public static string FirstCharToUpper(this string s) =>
    string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
}

