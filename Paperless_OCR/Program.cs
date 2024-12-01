namespace Paperless_OCR
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Registriere den OCRWorker als Hintergrunddienst
                    services.AddHostedService<OCRWorker>();
                });
    }

}