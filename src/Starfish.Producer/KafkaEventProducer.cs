namespace Starfish.Producer;

public class KafkaEventProducer : IKafkaEventProducer
{
    public void Send(string message)
    {
        Console.WriteLine(message);
    }
}