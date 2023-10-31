using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Starfish.Producer;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddScoped<IKafkaEventProducer, KafkaEventProducer>();
    services.AddLogging();

    services.Configure<KafkaConnectionSettings>(context.Configuration.GetSection(nameof(KafkaConnectionSettings)));
});

var host = await builder.StartAsync();

var producer = host.Services.GetRequiredService<IKafkaEventProducer>();
producer.Run();

