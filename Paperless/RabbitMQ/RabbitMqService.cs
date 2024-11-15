using RabbitMQ.Client;
using System;
using System.Text;


public class RabbitMqService
{
    private readonly string _hostName = "rabbitmq"; // Use "rabbitmq" to match the service name in docker-compose
    private readonly string _queueName = "document-queue";
    private readonly string _username = "user";
    private readonly string _password = "password";

    public void SendMessage(string message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _hostName,
            UserName = _username,
            Password = _password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        var body = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);

        Console.WriteLine(" [x] Sent {0}", message);
    }
}
