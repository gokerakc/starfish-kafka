using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using NJsonSchema.Generation;
using Starfish.Producer.Models;

namespace Starfish.Producer;

public class KafkaEventProducer : IKafkaEventProducer
{
    const string TopicName = "eu-west-2-basket-activities";


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
           // SubjectNameStrategy = SubjectNameStrategy.TopicRecord
        };

        var jsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings
        {
            SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            }
        };

        var latestSchema = await _schemaRegistryClient.GetRegisteredSchemaAsync($"{TopicName}-BasketActivity", 1);
        latestSchema.Schema.Subject = "eu-west-2-basket-activities-BasketActivity";

        // Error: System.ArgumentNullException: 'Value cannot be null. Arg_ParamName_Name'
        var jsonSeserializer = new JsonSerializer<BasketActivity>(_schemaRegistryClient, latestSchema,jsonSerializerConfig, jsonSchemaGeneratorSettings);

        using (var producer =
            new ProducerBuilder<string, BasketActivity>(_producerConfig)
                .SetValueSerializer(jsonSeserializer)
                .Build())
        {
            for (int i = 0; i < 10; i++)
            {

                BasketActivity basketActivity = new BasketActivity { UserId = $"x-{i}", ItemId = $"item{i}", ActivityType = ActivityType.Added, Quantity = 1 };
                try
                {
                    var a = await producer.ProduceAsync(TopicName, new Message<string, BasketActivity> { Key = $"x-{i}", Value = basketActivity });
                    Console.WriteLine(a.Offset);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"error producing message: {e.Message}");
                }
            }

            producer.Flush(TimeSpan.FromSeconds(10));
        }

    }
}