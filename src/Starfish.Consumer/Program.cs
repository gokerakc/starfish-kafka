using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Starfish.Consumer;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddScoped<IKafkaEventConsumer, KafkaEventConsumer>();
    services.AddLogging();

    services.Configure<KafkaConsumerSettings>(context.Configuration.GetSection(nameof(KafkaConsumerSettings)));
});


var host = await builder.StartAsync();

var applicationLifeTime = host.Services.GetRequiredService<IHostApplicationLifetime>();
var consumer = host.Services.GetRequiredService<IKafkaEventConsumer>();


var cancellationToken = RegisterCancellationToken(applicationLifeTime);
consumer.Run(cancellationToken);

CancellationToken RegisterCancellationToken(IHostApplicationLifetime applicationLifetime) {

    var cts = new CancellationTokenSource();
    var cancellationToken = cts.Token;
    applicationLifeTime!.ApplicationStopping.Register(() => { cts.Cancel(); });

    return cancellationToken;
}