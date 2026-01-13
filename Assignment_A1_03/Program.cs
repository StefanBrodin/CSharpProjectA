using Assignment_A1_03.Models;
using Assignment_A1_03.Services;

namespace Assignment_A1_03;

class Program
{
    static void Main(string[] args)
    {
        OpenWeatherService service = new OpenWeatherService();

        // Register the event
        service.WeatherForecastAvailable += Service_WeatherForecastAvailable;

        Task<Forecast>[] tasks = { null, null, null, null };
        Exception exception = null;
        try
        {
            double latitude = 59.5086798659495;
            double longitude = 18.2654625932976;

            // Create the two tasks and wait for comletion
            tasks[0] = service.GetForecastAsync(latitude, longitude);
            tasks[1] = service.GetForecastAsync("Miami");

            Task.WaitAll(tasks[0], tasks[1]); // Task.WaitAll är synkron och blockerar tråden till skillnad mot await Task.WhenAll

            tasks[2] = service.GetForecastAsync(latitude, longitude);
            tasks[3] = service.GetForecastAsync("Miami");

            // Wait and confirm we get an event showing cahced data avaialable
            Task.WaitAll(tasks[2], tasks[3]);
        }
        catch (Exception ex)
        {
            exception = ex;
            // How to handle an exception
            Console.WriteLine("Weather service error.");
            Console.WriteLine($"Error: {exception.Message}");
        }

        foreach (var task in tasks)
        {
            // How to deal with successful and fault tasks
            if (task.IsCompletedSuccessfully)
            {
                var forecast = task.Result;

                string forecastTitle = $"Väderprognos för {forecast.City}";
                Console.WriteLine(forecastTitle);
                Console.WriteLine(new string('-', forecastTitle.Length));

                var groupedByDay = forecast.Items
                    .GroupBy(f => f.DateTime.Date)
                    .OrderBy(g => g.Key);

                foreach (var groupOfDays in groupedByDay)
                {
                    string forecastDate = groupOfDays.Key.ToString("dddd dd MMMM yyyy").FirstCharToUpper();
                    Console.WriteLine(forecastDate);

                    // Now the printout can handle potential missing values
                    foreach (var item in groupOfDays)
                    {
                        string timeStr = item.DateTime.ToString("HH:mm");
                        string tempStr = item.Temperature?.ToString("F1") ?? "(inget värde)";
                        string windStr = item.WindSpeed?.ToString("F1") ?? "(inget värde)";
                        string descStr = item.Description?.FirstCharToUpper() ?? "(inget värde)";

                        Console.WriteLine($"   - {timeStr}: {descStr}, temperatur: {tempStr} °C, vind: {windStr} m/s.");
                    }

                    Console.WriteLine();
                }

            }
            else if (task.IsFaulted)
            {
                Console.WriteLine($"Task failed: {task.Exception?.InnerException?.Message ?? "Unknown error"}");
            }
        }
    }


    // Event handler declaration
    private static void Service_WeatherForecastAvailable(object sender, string message)
    {
        Console.WriteLine($"Event message from weather service: {message}");
    }
}

public static class MyExtensions
{
    public static string FirstCharToUpper(this string s) =>
    string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
}
