using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NJsonSchema.Generation;
using Starfish.Consumer.Models;

namespace Starfish.Consumer;

public class KafkaEventConsumer : IKafkaEventConsumer
{
    private const string TopicName = "eu-west-2-basket-activities";

    private readonly ISchemaRegistryClient _schemaRegistryClient;
    private readonly ConsumerConfig _consumerConfig;


    public KafkaEventConsumer(ISchemaRegistryClient schemaRegistryClient, IOptions<KafkaConsumerSettings> options)
    {
        _schemaRegistryClient = schemaRegistryClient;
        
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


    public async Task Run(CancellationToken cancellationToken)
    {
        var jsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings
        {
            SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                }
            }
        };

        var latestSchema = await _schemaRegistryClient.GetRegisteredSchemaAsync($"{TopicName}-BasketActivity", 3);

        var jsonDeserializer = new JsonDeserializer<BasketActivity>(_schemaRegistryClient, latestSchema.Schema, null, jsonSchemaGeneratorSettings);

        using (var consumer = new ConsumerBuilder<string, BasketActivity>(_consumerConfig)
                   .SetKeyDeserializer(Deserializers.Utf8)
                   .SetValueDeserializer(jsonDeserializer.AsSyncOverAsync())
                   .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
                   .Build())
        {
            consumer.Subscribe(TopicName);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(cancellationToken);

                    var activity = consumeResult.Message.Value as BasketActivity;

                    Console.WriteLine($"Partition: {consumeResult.Partition} | Value: {activity} | Offset: {consumeResult.Offset} | TopicPartitionOffset: {consumeResult.TopicPartitionOffset}");
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