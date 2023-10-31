using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Starfish.Producer;

public class KafkaEventProducer : IKafkaEventProducer
{
    private readonly ProducerConfig _producerConfig;
    public KafkaEventProducer(IOptions<KafkaConnectionSettings> options)
    {
        var settings = options.Value;

        _producerConfig = new ProducerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            SaslUsername = settings.SaslUsername,
            SaslPassword = settings.SaslPassword,
            SaslMechanism = settings.SaslMechanisms,
            SecurityProtocol = settings.SecurityProtocol,
        };
    }
    
    public void Run()
    {
        const string topic = "test";

        string[] users = { "eabara", "jsmith", "sgarcia", "jbernard", "htanaka", "awalther" };
        string[] items = { "book", "alarm clock", "t-shirts", "gift card", "batteries" };
        
        using (var producer = new ProducerBuilder<string, string>(_producerConfig).Build())
        {
            var numProduced = 0;
            var rnd = new Random();
            const int numMessages = 10;
            for (var i = 0; i < numMessages; ++i)
            {
                var user = users[rnd.Next(users.Length)];
                var item = items[rnd.Next(items.Length)];

                producer.Produce(topic, new Message<string, string>
                    {
                        Key = user,
                        Value = item
                    },
                    (deliveryReport) =>
                    {
                        if (deliveryReport.Error.Code != ErrorCode.NoError)
                        {
                            Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
                        }
                        else
                        {
                            Console.WriteLine($"Produced event to topic {topic}: key = {user,-10} value = {item}");
                            numProduced += 1;
                        }
                    });
            }

            producer.Flush(TimeSpan.FromSeconds(10));
            Console.WriteLine($"{numProduced} messages were produced to topic {topic}");
        }
    }
}