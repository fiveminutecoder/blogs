using FiveMinuteProject.Data.Interfaces;

namespace FiveMinuteProject.Data
{
    public class WeatherRepository : IWeatherRepository
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        
        static Random random = new Random(5);

        /// <summary>
        /// Returns an array of WeatherForecast objects
        /// representing the weather for the next Range days.
        /// </summary>
        public WeatherForecast[] GetForecast(int Range)
        {
            var rng = new Random();
            return Enumerable.Range(1, Range).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = random.Next(-20, 55),
                Summary = Summaries[random.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}