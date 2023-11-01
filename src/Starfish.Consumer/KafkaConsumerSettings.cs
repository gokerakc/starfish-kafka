using Confluent.Kafka;

namespace Starfish.Consumer;

public class KafkaConsumerSettings
{
    public string? BootstrapServers { get; set; }
    
    public string? GroupId { get; set; }
    
    public string? SaslUsername { get; set; }
    
    public string? SaslPassword { get; set; }

    public AutoOffsetReset AutoOffsetReset { get; set; }

    public SecurityProtocol SecurityProtocol { get; set; }

    public SaslMechanism SaslMechanisms { get; set; }
}