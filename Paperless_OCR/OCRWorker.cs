using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tesseract;

public class OCRWorker : BackgroundService
{
    private readonly ILogger<OCRWorker> _logger;
    private readonly IConnection _rabbitConnection;
    private readonly IModel _rabbitChannel;

    public OCRWorker(ILogger<OCRWorker> logger)
    {
        _logger = logger;

        // Verbindung zu RabbitMQ
        var factory = new ConnectionFactory() { HostName = "rabbitmq" };  // Hostname sollte dem Docker-Container von RabbitMQ entsprechen
        _rabbitConnection = factory.CreateConnection();
        _rabbitChannel = _rabbitConnection.CreateModel();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _rabbitChannel.QueueDeclare(queue: "ocrQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        var consumer = new EventingBasicConsumer(_rabbitChannel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Nachricht erhalten: {0}", message);

            // OCR-Prozess durchführen
            var result = await PerformOCRAsync(message);

            // Ergebnis verarbeiten
            _logger.LogInformation("OCR-Ergebnis: {0}", result);
        };

        _rabbitChannel.BasicConsume(queue: "ocrQueue", autoAck: true, consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private Task<string> PerformOCRAsync(string filePath)
    {
        return Task.Run(() =>
        {
            using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(filePath);
            using var page = engine.Process(img);
            return page.GetText();
        });
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OCR Worker wird gestoppt.");
        _rabbitChannel.Close();
        _rabbitConnection.Close();

        return base.StopAsync(cancellationToken);
    }
}