using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Starfish.Consumer;

public class KafkaEventConsumer : IKafkaEventConsumer
{

    private readonly ConsumerConfig _consumerConfig;

    private const string Topic = "eu-west-2-demo";

    public KafkaEventConsumer(IOptions<KafkaConsumerSettings> options)
    {
        var settings = options.Value;

        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            SaslUsername = settings.SaslUsername,
            SaslPassword = settings.SaslPassword,
            SaslMechanism = settings.SaslMechanisms,
            SecurityProtocol = settings.SecurityProtocol,
        };
    }


    public void Run(CancellationToken cancellationToken)
    {

        using (var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build())
        {
            consumer.Subscribe(Topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(cancellationToken);

                    Console.WriteLine($"Partition: {consumeResult.Partition} | Message: {consumeResult.Message.Value} | Offset: {consumeResult.Offset} | TopicPartitionOffset: {consumeResult.TopicPartitionOffset}");
                }
                catch (OperationCanceledException)
                {

                    Console.WriteLine("Consume operation has been cancelled");
                }
                
            }

            Console.WriteLine("Stopped consuming messages...");

            consumer.Close();

        }
    }
}