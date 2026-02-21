using Microsoft.EntityFrameworkCore;
using MusicStreamerBackend.Data;
using MusicStreamerBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IMusicInfoService, MusicInfoService>();
builder.Services.AddScoped<IDbStorageService, DbStorageService>();
builder.Services.AddScoped<IStreamingService, StreamingService>();
builder.Services.AddDbContext<MusicStreamerDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("MusicStreamerDb"));
});

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
});

builder.Services.AddHttpClient("Discogs", client =>
{
    client.BaseAddress = new Uri("https://api.discogs.com/");
    client.DefaultRequestHeaders.Add("User-Agent", $"MusicStreamerBackend/0.1");
    var key = builder.Configuration["Discogs:ConsumerKey"];
    var secret = builder.Configuration["Discogs:ConsumerSecret"];
    if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
    {
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
        logger.LogError("Discogs API credentials are not configured. Please set 'Discogs:ConsumerKey' and 'Discogs:ConsumerSecret' in the configuration.");
        throw new ArgumentException("Discogs API credentials are not configured. Please set 'Discogs:ConsumerKey' and 'Discogs:ConsumerSecret' in the configuration.");
    }
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Discogs", $"key={key}, secret={secret}");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OrchestratorV2 API v1");
    });
}
else
{
    app.UseHttpsRedirection();
}
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request from: {context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}");
    await next();
});

app.UseAuthorization();
app.MapControllers();
app.Run();