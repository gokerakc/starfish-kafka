namespace Starfish.Consumer;

public interface IKafkaEventConsumer
{
    public Task Run(CancellationToken cancellationToken);
}