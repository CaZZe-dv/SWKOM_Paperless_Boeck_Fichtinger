using RabbitMQ.Client;
using System;
using System.Text;

public class RabbitMqService : IDisposable
{
    private readonly string _hostName = "rabbitmq"; // Use "rabbitmq" to match the service name in docker-compose
    private readonly string _queueName = "ocrQueue";
    private readonly string _username = "guest";
    private readonly string _password = "guest";

    private IConnection _connection;
    private IModel _channel;

    // Constructor: Initializes the RabbitMQ connection and channel
    public RabbitMqService()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _hostName,
            UserName = _username,
            Password = _password
        };

        try
        {
            // Create connection and channel
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare the queue
            _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            Console.WriteLine("Queue {0} declared successfully.", _queueName);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error initializing RabbitMQ connection: {ex.Message}");
            throw;
        }
    }

    // Method to send a message to the RabbitMQ queue
    public void SendMessage(string message)
    {
        try
        {
            // Convert the message to a byte array
            var body = Encoding.UTF8.GetBytes(message);

            // Publish the message to the queue
            _channel.BasicPublish(exchange: "",
                                  routingKey: _queueName,
                                  basicProperties: null,
                                  body: body);

            Console.WriteLine(" [x] Sent {0}", message);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    // Method to dispose of the RabbitMQ connection and channel
    public void Dispose()
    {
        try
        {
            // Close the channel and connection if they are open
            _channel?.Close();
            _connection?.Close();
            Console.WriteLine("RabbitMQ connection and channel closed.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error while disposing RabbitMQ resources: {ex.Message}");
        }
    }
}