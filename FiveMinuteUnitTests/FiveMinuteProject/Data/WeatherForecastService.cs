namespace FiveMinuteProject.Data;

using FiveMinuteProject.Data.Interfaces;


public class WeatherForecastService
{

    IWeatherRepository _weatherRepository;

    public WeatherForecastService(IWeatherRepository weatherRepository)
    {
        _weatherRepository = weatherRepository;
    }



    public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
    {
        int range = 5;
        DateTime now = DateTime.Now;

        if(startDate > now)
            throw new ArgumentException("startDate cannot be in the future", nameof(startDate));

        range = now.Subtract(startDate).Days;
        
        return Task.FromResult(_weatherRepository.GetForecast(range));
    }
}
