using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Starfish.Consumer;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    services.AddScoped<IKafkaEventConsumer, KafkaEventConsumer>();
    services.AddLogging();
});

var host = await builder.StartAsync();

var consumer = host.Services.GetRequiredService<IKafkaEventConsumer>();
consumer.Run();