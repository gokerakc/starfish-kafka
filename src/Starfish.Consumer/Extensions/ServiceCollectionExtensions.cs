using Confluent.SchemaRegistry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Starfish.Consumer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSchemaRegistry(this IServiceCollection services, IConfiguration config)
    {
        var settings = config.GetValue<KafkaConsumerSettings>(nameof(KafkaConsumerSettings));
        
        var schemaRegistryConfig = new SchemaRegistryConfig
        {
            Url = settings.SchemaRegistryUrl,
            BasicAuthUserInfo = settings.SchemaRegistryAuth
        };

        var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);
        services.AddSingleton<ISchemaRegistryClient>(schemaRegistryClient);
        
        return services;
    }
}