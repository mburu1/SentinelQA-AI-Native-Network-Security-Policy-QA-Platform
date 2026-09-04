namespace SentinelQA.Infrastructure.Messaging;

public sealed class MessagingOptions
{
    public bool Enabled { get; set; } = true;
}

public sealed class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "sentinelqa";
    public string Password { get; set; } = "sentinelqa";

    public string ConnectionString => $"amqp://{Username}:{Password}@{Host}:{Port}/";
}

public sealed class KafkaOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";
}