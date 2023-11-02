using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Starfish.Consumer;
using Starfish.Consumer.Extensions;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddScoped<IKafkaEventConsumer, KafkaEventConsumer>();
    services.AddSchemaRegistry(context.Configuration);
    services.AddLogging();

    services.Configure<KafkaConsumerSettings>(context.Configuration.GetSection(nameof(KafkaConsumerSettings)));
});


var host = await builder.StartAsync();

var applicationLifeTime = host.Services.GetRequiredService<IHostApplicationLifetime>();
var cts = new CancellationTokenSource();
applicationLifeTime.ApplicationStopping.Register(() => { cts.Cancel(); });

var consumer = host.Services.GetRequiredService<IKafkaEventConsumer>();

await consumer.Run(cts.Token);

