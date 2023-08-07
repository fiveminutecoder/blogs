using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using FiveMinuteProject.Data;
using FiveMinuteProject.Data.Interfaces;

namespace FiveMinuteProject.Tests
{
    public class WeatherRepositoryStubs
    {
        Mock<IWeatherRepository> mockWeatherRepository = new Mock<IWeatherRepository>();
        //Create a stub for IWeatherRepository using Moq

        public IWeatherRepository BuildStub()
        {
            mockWeatherRepository.Setup(x => x.GetForecast(It.IsAny<int>()))
                .Returns(new WeatherForecast[]
                {
                    new WeatherForecast
                    {
                        Date = DateTime.Now,
                        TemperatureC = 32,
                        Summary = "Freezing"
                    }
                });
            return mockWeatherRepository.Object;
        }

    }
}