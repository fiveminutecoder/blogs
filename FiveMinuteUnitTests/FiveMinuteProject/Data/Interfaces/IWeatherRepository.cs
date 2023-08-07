using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FiveMinuteProject.Data.Interfaces
{
    public interface IWeatherRepository
    {
        WeatherForecast[] GetForecast(int Range);
    }
}