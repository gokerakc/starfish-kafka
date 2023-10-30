namespace Starfish.Consumer;

public interface IKafkaEventConsumer
{
    public Task ReceiveAsync();
}