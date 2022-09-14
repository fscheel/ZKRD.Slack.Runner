using System.Text.Json.Serialization;

namespace Zkrd.Slack.Weather.Responses;

public class Daily
{
   [JsonPropertyName("temperature_2m_min")]
   public double[] Temperature2mMin { get; set; }
   [JsonPropertyName("temperature_2m_max")]
   public double[] Temperature2mMax { get; set; }
   public DateTime[] Time { get; set; }
   public decimal[] Weathercode { get; set; }
}
