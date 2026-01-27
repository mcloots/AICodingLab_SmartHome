using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartHome.Plugins
{
    public class WeatherPlugin
    {

        private static readonly HttpClient _httpClient = new();

        // https://api.open-meteo.com/v1/forecast?latitude=51.160858&longitude=4.961980&current=temperature_2m,weather_code

        [KernelFunction("get_weather")]
        [Description("Gets the current weather using latitude and longitude using real data from the Open-Meteo API")]
        public async Task<string> GetWeatherAsync(double lat, double lon)
        {
            string latStr = lat.ToString(CultureInfo.InvariantCulture);
            string lonStr = lon.ToString(CultureInfo.InvariantCulture);

            var url = $"https://api.open-meteo.com/v1/forecast?latitude={latStr}&longitude={lonStr}&current=temperature_2m,weather_code";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                // get the current temperature and weatherCode
                var current = doc.RootElement.GetProperty("current");
                var temperature = current.GetProperty("temperature_2m").GetDouble();
                var weatherCode = current.GetProperty("weather_code").GetInt32();

                var weatherDescription = weatherCode switch
                {
                    0 => "Clear sky",
                    1 or 2 or 3 => "Partly cloudy",
                    45 or 48 => "Foggy",
                    51 or 53 or 55 => "Drizzle",
                    61 or 63 or 65 => "Rain",
                    71 or 73 or 75 => "Snow",
                    95 => "Thunderstorm",
                    _ => "Unknown weather"
                };

                return $"The weather at latitude: {lat} and longitude: {lon} is {weatherDescription} with a temperature of {temperature:F1}°C.";
            }
            catch (Exception ex)
            {
                return $"Failed to get weather data for latitude: {lat} and longitude: {lon}: {ex.Message}";
            }
        }

    }
}
