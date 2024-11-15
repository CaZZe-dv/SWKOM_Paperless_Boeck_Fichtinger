using log4net.Config;
using log4net.Repository;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Paperless.Database;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// log4net-Setup
ILoggerRepository logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
var logger = LogManager.GetLogger(typeof(Program));

logger.Info("Anwendung wird gestartet...");

//For Mapping purposes
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
//Postgresql configuration
var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DatabaseDbContext>(options => options.UseNpgsql(conn));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Paperless API",
        Version = "v1",
        Description = "Paperless API"
    });

    //xml kommentare   in Paperless.csproj aktiviert
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

});

var app = builder.Build();

// normalerweise swagger nicht für produktiv; später '!' ändern
if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


// Globale Fehlerbehandlung mit log4net
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        logger.Error("Ein unerwarteter Fehler ist aufgetreten.", ex);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Interner Serverfehler. Bitte versuchen Sie es später erneut." });
    }
});

// Beispiel-Log für erfolgreichen Start
logger.Info("Paperless Application gestartet und läuft auf Port 8081");



// Configure Kestrel to listen on port 8081
app.Urls.Add("http://*:8081");

app.Run();
