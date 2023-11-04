using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema.Generation;
using Starfish.Producer.Models;

namespace Starfish.Producer;

public class KafkaEventProducer : IKafkaEventProducer
{
    private const string TopicName = "eu-west-2-basket-activities";

    private readonly ISchemaRegistryClient _schemaRegistryClient;
    private readonly ProducerConfig _producerConfig;

    public KafkaEventProducer(ISchemaRegistryClient schemaRegistryClient, IOptions<KafkaProducerSettings> options)
    {
        _schemaRegistryClient = schemaRegistryClient;

        var settings = options.Value;

        _producerConfig = new ProducerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            SaslUsername = settings.SaslUsername,
            SaslPassword = settings.SaslPassword,
            SaslMechanism = settings.SaslMechanisms,
            SecurityProtocol = settings.SecurityProtocol,
            ClientId = "starfish-producer"
        };
    }
    
    public async Task Run(CancellationToken cancellationToken)
    {
        var jsonSerializerConfig = new JsonSerializerConfig
        {
            BufferBytes = 100,
            UseLatestVersion = true,
            AutoRegisterSchemas = false,
            SubjectNameStrategy = SubjectNameStrategy.TopicRecord,
        };

        var jsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings
        {
            SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                },
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(new CamelCaseNamingStrategy()),
                },
            },
        };

        var latestSchema = await _schemaRegistryClient.GetRegisteredSchemaAsync($"{TopicName}-BasketActivity", 3);
        var jsonSerializer = new JsonSerializer<BasketActivity>(_schemaRegistryClient, latestSchema,jsonSerializerConfig, jsonSchemaGeneratorSettings);

        using (var producer =
            new ProducerBuilder<string, BasketActivity>(_producerConfig)
                .SetValueSerializer(jsonSerializer.AsSyncOverAsync())
                .Build())
        {
            for (var i = 0; i < 10; i++)
            {
                BasketActivity basketActivity = new BasketActivity { UserId = $"x-{i}", ItemId = $"item{i}", ActivityType = ActivityType.Added, Quantity = 1, Timestamp = DateTime.UtcNow};

                try
                {
                    var message = new Message<string, BasketActivity> { Key = $"x-{i}", Value = basketActivity };

                    var deliveryResult = await producer.ProduceAsync(TopicName, message, cancellationToken);

                    Console.WriteLine(deliveryResult.Offset);
                }
                catch (KafkaException e)
                {
                    Console.WriteLine($@"An error occured while producing events. Message: ""{e.Message}"" InnerException: ""{e.InnerException}""");
                }
                catch (OperationCanceledException)
                {

                    Console.WriteLine("Produce operation has been cancelled");
                    break;
                }
            }

            producer.Flush(TimeSpan.FromSeconds(10));
            Console.WriteLine("All produce processes are completed.");
        }

    }
}