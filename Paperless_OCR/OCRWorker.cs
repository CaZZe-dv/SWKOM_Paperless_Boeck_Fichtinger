using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class OCRWorker : BackgroundService
{
    private readonly ILogger<OCRWorker> _logger;
    private readonly IConnection _rabbitConnection;
    private readonly IModel _rabbitChannel;
    private readonly IMinioClient _minioClient;
    private const string QueueName = "ocrQueue";
    private const string BucketName = "uploads"; // MinIO bucket name

    public OCRWorker(ILogger<OCRWorker> logger)
    {
        _logger = logger;

        // MinIO-Client erstellen
        _minioClient = new MinioClient()
            .WithEndpoint("minio", 9000)
            .WithCredentials("minioadmin", "minioadmin")
            .WithSSL(false)
            .Build();

        // Verbindung zu RabbitMQ aufbauen
        var factory = new ConnectionFactory()
        {
            HostName = "rabbitmq",
            UserName = "guest",
            Password = "guest"
        };

        try
        {
            _rabbitConnection = factory.CreateConnection();
            _rabbitChannel = _rabbitConnection.CreateModel();

            // Warteschlange deklarieren
            _rabbitChannel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false);
            _logger.LogInformation("RabbitMQ verbunden und Warteschlange '{QueueName}' bereit.", QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Verbinden mit RabbitMQ.");
            throw;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_rabbitChannel);

        // Event-Handler für eingehende Nachrichten
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var parts = message.Split('|');

            if (parts.Length == 2)
            {
                var id = parts[0];
                var fileName = parts[1];
                _logger.LogInformation("Nachricht erhalten: {Id}, {FileName}", id, fileName);

                try
                {
                    // Lade die Datei von MinIO herunter
                    var localFilePath = await DownloadFileFromMinIOAsync(fileName);

                    // OCR-Verarbeitung durchführen
                    var extractedText = await PerformOcrAsync(localFilePath);

                    if (!string.IsNullOrEmpty(extractedText))
                    {
                        // OCR-Ergebnis zurück an RabbitMQ senden
                        var resultMessage = $"{id}|{extractedText.Replace("\n", " ").Replace("\r", "")}";
                        var resultBody = Encoding.UTF8.GetBytes(resultMessage);
                        _rabbitChannel.BasicPublish(exchange: "", routingKey: "ocrResultQueue", basicProperties: null, body: resultBody);

                        _logger.LogInformation("OCR-Ergebnis für ID: {Id} an Warteschlange gesendet.", id);
                    }

                    // Aufräumen der lokalen Datei
                    if (File.Exists(localFilePath))
                    {
                        File.Delete(localFilePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler bei der Verarbeitung der Datei {FileName}", fileName);
                }
            }
            else
            {
                _logger.LogError("Ungültige Nachricht empfangen, konnte nicht in 2 Teile aufgeteilt werden.");
            }

            // Bestätigung der Verarbeitung
            _rabbitChannel.BasicAck(ea.DeliveryTag, false);
        };

        _rabbitChannel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        // Unendliche Schleife, solange der Worker läuft
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken); // Verhindert Busy-Waiting
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OCR Worker wird gestoppt.");
        _rabbitChannel?.Close();
        _rabbitConnection?.Close();

        return base.StopAsync(cancellationToken);
    }

    // Methode zum Herunterladen der Datei von MinIO
    private async Task<string> DownloadFileFromMinIOAsync(string fileName)
    {
        var localFilePath = Path.Combine(Path.GetTempPath(), fileName);

        try
        {
            using var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(fileName)
                .WithCallbackStream(stream => stream.CopyTo(fileStream)));

            _logger.LogInformation("Datei {FileName} nach {LocalFilePath} heruntergeladen", fileName, localFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Herunterladen der Datei {FileName} von MinIO", fileName);
            throw;
        }

        return localFilePath;
    }

    // Methode zur Durchführung der OCR-Verarbeitung
    public async Task<string> PerformOcrAsync(string filePath)
    {
        var stringBuilder = new StringBuilder();

        try
        {
            using (var images = new ImageMagick.MagickImageCollection(filePath)) // Für mehrseitige Dokumente
            {
                foreach (var image in images)
                {
                    var tempPngFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");

                    image.Density = new ImageMagick.Density(300, 300); // Auflösung setzen
                    image.Format = ImageMagick.MagickFormat.Png;
                    image.Write(tempPngFile);

                    // Tesseract-CLI für jede Seite ausführen
                    var psi = new ProcessStartInfo
                    {
                        FileName = "tesseract", // Annahme: Tesseract ist im PATH
                        Arguments = $"{tempPngFile} stdout -l eng",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    try
                    {
                        using (var process = Process.Start(psi))
                        {
                            if (process == null)
                            {
                                throw new InvalidOperationException("Fehler beim Starten des Tesseract-Prozesses.");
                            }

                            string result = await process.StandardOutput.ReadToEndAsync();
                            stringBuilder.Append(result);
                        }
                    }
                    finally
                    {
                        if (File.Exists(tempPngFile))
                        {
                            File.Delete(tempPngFile); // Temporäre PNG-Datei nach der Verarbeitung löschen
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der OCR-Verarbeitung");
        }

        return stringBuilder.ToString();
    }
}
