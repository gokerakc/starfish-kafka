using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Starfish.Consumer.Models;

public record BasketActivity
{
    [JsonProperty("activityType")]
    public ActivityType ActivityType { get; set; }

    [JsonProperty("itemId")]
    public string ItemId { get; set; } = string.Empty;

    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonProperty("quantity")]
    public int Quantity { get; set; }

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
}

public enum ActivityType
{
    None = 0,
    
    Added = 1,
    
    Deleted = 2,
    
    Updated = 3,
}