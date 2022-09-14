using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Zkrd.Slack.Weather.Converter;
using Zkrd.Slack.Weather.Responses;

namespace Zkrd.Slack.Weather.Tests;

[TestFixture]
public class WeatherResponseConverterTests
{
   [Test]
   public void Converter_Should_ConvertResponseObjectToArrayOfWeatherObjects()
   {
      var response = new OpenMeteoResponse
      {
         Daily = new Daily
         {
            Temperature2mMin = new[] { -7.5, -9.7, -14.6, -16.2, -14, -12.4, -6.8 },
            Temperature2mMax = new[] { -6, -7.5, -9.9, -12, -9.9, -5.3, -2.8 },
            Time = new[]
            {
               DateTime.Parse("2022-03-31"), DateTime.Parse("2022-04-01"), DateTime.Parse("2022-04-02"),
               DateTime.Parse("2022-04-03"), DateTime.Parse("2022-04-04"), DateTime.Parse("2022-04-05"),
               DateTime.Parse("2022-04-06")
            },
            Weathercode = new[] { 85m, 86, 85, 71, 71, 3, 85 },
         }
      };
      var sut = new WeatherResponseConverter();

      IEnumerable<WeatherData> convertedObject = sut.Convert(response);

      convertedObject.Should().HaveCount(7);
   }
   
   [Test]
   public void Converter_Should_HaveFirstWeatherObjectWithData()
   {
      var response = new OpenMeteoResponse
      {
         Daily = new Daily
         {
            Temperature2mMin = new[] { -7.5 },
            Temperature2mMax = new[] { -6.0 },
            Time = new[]
            {
               DateTime.Parse("2022-03-31"),
            },
            Weathercode = new[] { 85m },
         }
      };
      var sut = new WeatherResponseConverter();

      IEnumerable<WeatherData> convertedObject = sut.Convert(response);

      convertedObject.First().Should().BeEquivalentTo(new WeatherData
      {
         MaximumTemperature = -6,
         MinimumTemperature = -7.5,
         Date = new DateOnly(2022, 03, 31),
         Condition = "slight snow showers",
      });
   }

   [TestCase(0, "clear")]
   [TestCase(1, "mainly clear")]
   [TestCase(2, "partly cloudy")]
   [TestCase(3, "overcast")]
   [TestCase(45, "fog")]
   [TestCase(48, "depositing rime fog")]
   [TestCase(51, "light drizzle")]
   [TestCase(53, "moderate drizzle")]
   [TestCase(55, "dense drizzle")]
   [TestCase(56, "light freezing drizzle")]
   [TestCase(57, "dense freezing drizzle")]
   [TestCase(61, "slight rain")]
   [TestCase(63, "moderate rain")]
   [TestCase(65, "heavy rain")]
   [TestCase(66, "light freezing rain")]
   [TestCase(67, "heavy freezing rain")]
   [TestCase(71, "slight snowfall")]
   [TestCase(73, "moderate snowfall")]
   [TestCase(75, "heavy snowfall")]
   [TestCase(77, "snow grains")]
   [TestCase(80, "slight rain showers")]
   [TestCase(81, "moderate rain showers")]
   [TestCase(82, "violent rain showers")]
   [TestCase(85, "slight snow showers")]
   [TestCase(86, "heavy snow showers")]
   [TestCase(95, "thunderstorm")]
   [TestCase(96, "thunderstorm with slight hail")]
   [TestCase(99, "thunderstorm with heavy hail")]
   [TestCase(100000, "Unknown weather condition 100000")]
   public void Converter_Should_HaveFirstWeatherObjectWithData(decimal weatherCode, string expectedWeatherStatus)
   {
      var response = new OpenMeteoResponse
      {
         Daily = new Daily
         {
            Temperature2mMin = new[] { -7.5 },
            Temperature2mMax = new[] { -6.0 },
            Time = new[]
            {
               DateTime.Parse("2022-03-31"),
            },
            Weathercode = new[] { weatherCode },
         }
      };
      var sut = new WeatherResponseConverter();

      IEnumerable<WeatherData> convertedObject = sut.Convert(response);

      convertedObject.First().Should().BeEquivalentTo(new WeatherData
      {
         MaximumTemperature = -6,
         MinimumTemperature = -7.5,
         Date = new DateOnly(2022, 03, 31),
         Condition = expectedWeatherStatus,
      });
   }
}
