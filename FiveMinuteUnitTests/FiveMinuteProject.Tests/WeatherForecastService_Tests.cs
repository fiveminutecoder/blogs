using System;
using NUnit.Framework;
using System.Threading.Tasks;
using FiveMinuteProject;
using FiveMinuteProject.Data;
using FiveMinuteProject.Data.Interfaces;


namespace FiveMinuteProject.Tests;

[TestFixture]
public class WeatherForecastService_Tests
{
    //create test cases for the following: WeatherForecast[] GetForecastAsync(DateTime startDate, int range)
    //1. startDate is in the future
    //2. startDate is in the past
    //3. startDate is today
    //4. startDate is DateTime.MinValue
    //5. startDate is DateTime.MaxValue

    WeatherForecastService weatherForecastService;

    [SetUp]
    public void Setup()
    {
        WeatherRepositoryStubs weatherRepositoryStubs = new WeatherRepositoryStubs();
        IWeatherRepository weatherRepository = weatherRepositoryStubs.BuildStub();
        weatherForecastService = new WeatherForecastService(weatherRepository);
    }

    
    
    [Test]
    public void GetForecastAsync_StartDateIsInTheFuture_ThrowsArgumentException()
    {
        //Arrange
        
        DateTime startDate = DateTime.Now.AddDays(1);
        
        //Act
        //Assert
        Assert.Throws<ArgumentException>(() => weatherForecastService.GetForecastAsync(startDate));
    }

    [Test]
    public async Task GetForecastAsync_StartDateIsInThePast_ReturnsWeatherForecastArray()
    {
        //Arrange
        DateTime startDate = DateTime.Now.AddDays(-1);
        
        //Act
        var result = await weatherForecastService.GetForecastAsync(startDate);
        
        //Assert
        Assert.IsInstanceOf<WeatherForecast[]>(result);
    }

    [Test]
    public async Task GetForecastAsync_StartDateIsToday_ReturnsWeatherForecastArray()
    {
        //Arrange
        DateTime startDate = DateTime.Now;
        
        //Act
        var result = await weatherForecastService.GetForecastAsync(startDate);
        
        //Assert
        Assert.IsInstanceOf<WeatherForecast[]>(result);
    }


    [Test]
    public async Task GetForecastAsync_StartDateIsDateTimeMinValue_ReturnsWeatherForecastArray()
    {
        //Arrange
        DateTime startDate = DateTime.MinValue;
        
        //Act
        var result = await weatherForecastService.GetForecastAsync(startDate);
        
        //Assert
        Assert.IsInstanceOf<WeatherForecast[]>(result);
    }

    [Test]
    public void GetForecastAsync_StartDateIsDateTimeMaxValue_ReturnsWeatherForecastArray()
    {
        //Arrange
        DateTime startDate = DateTime.MaxValue;
        
        //Act
        //Assert
        Assert.Throws<ArgumentException>(() => weatherForecastService.GetForecastAsync(startDate));
    }

}