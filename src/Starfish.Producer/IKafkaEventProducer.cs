namespace Starfish.Producer;

public interface IKafkaEventProducer
{
    public Task Run(CancellationToken cancellationToken);
}