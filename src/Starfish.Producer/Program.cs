using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Starfish.Producer;
using Starfish.Producer.Extensions;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddScoped<IKafkaEventProducer, KafkaEventProducer>();
    services.AddSchemaRegistry(context.Configuration);
    services.AddLogging();

    services.Configure<KafkaProducerSettings>(context.Configuration.GetSection(nameof(KafkaProducerSettings)));
});

var host = await builder.StartAsync();

var applicationLifeTime = host.Services.GetRequiredService<IHostApplicationLifetime>();
var producer = host.Services.GetRequiredService<IKafkaEventProducer>();

var cancellationToken = RegisterCancellationToken(applicationLifeTime);

await producer.Run(cancellationToken);


CancellationToken RegisterCancellationToken(IHostApplicationLifetime applicationLifetime)
{
    var cts = new CancellationTokenSource();
    var cancellationToken = cts.Token;
    applicationLifeTime!.ApplicationStopping.Register(() => { cts.Cancel(); });

    return cancellationToken;
}
