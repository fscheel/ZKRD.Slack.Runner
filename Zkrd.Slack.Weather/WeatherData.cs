namespace Zkrd.Slack.Weather;

public class WeatherData
{
   public double MaximumTemperature { get; init; }
   public double MinimumTemperature { get; init; }
   public DateOnly Date { get; init; }
   public string Condition { get; init; }
}
