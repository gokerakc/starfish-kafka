using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Starfish.Producer;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    services.AddScoped<IKafkaEventProducer, KafkaEventProducer>();
    services.AddLogging();
});

var host = await builder.StartAsync();

var producer = host.Services.GetRequiredService<IKafkaEventProducer>();
producer.Send("Hello world!");

