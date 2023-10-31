using Confluent.Kafka;

namespace Starfish.Producer;

public class KafkaConnectionSettings
{
    public string BootstrapServers { get; set; }
    
    public SecurityProtocol SecurityProtocol { get; set; }
    
    public SaslMechanism SaslMechanisms { get; set; }
    
    public string SaslUsername { get; set; }
    
    public string SaslPassword { get; set; }
}