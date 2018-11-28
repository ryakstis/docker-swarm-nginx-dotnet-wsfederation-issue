using System.Collections.Generic;
using wsfed_issue.Models;

namespace wsfed_issue.Providers
{
    public interface IWeatherProvider
    {
        List<WeatherForecast> GetForecasts();
    }
}
