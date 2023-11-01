namespace Starfish.Consumer;

public interface IKafkaEventConsumer
{
    public void Run(CancellationToken cancellationToken);
}