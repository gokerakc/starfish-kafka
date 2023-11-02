namespace Starfish.Consumer.Models;

public class BasketActivity
{
    public ActivityType ActivityType { get; set; }
    
    public string ItemId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    
    public int Quantity { get; set; }
    
    public DateTime Timestamp { get; set; }
}

public enum ActivityType
{
    None = 0,
    
    Added = 1,
    
    Deleted = 2,
    
    Updated = 3,
}