namespace Starfish.Producer;

public interface IKafkaEventProducer
{
    public void Send(string message);
}