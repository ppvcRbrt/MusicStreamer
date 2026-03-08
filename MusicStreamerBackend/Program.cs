using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MusicStreamerBackend.Data;
using MusicStreamerBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IMusicBrainzService, MusicBrainzService>();
builder.Services.AddScoped<IDbStorageService, DbStorageService>();
builder.Services.AddScoped<IMusicService, MusicService>();
builder.Services.AddScoped<IExternalMetadataService, ExternalMetadataService>();

builder.Services.AddSingleton<AlbumMetadataSyncService>();
builder.Services.AddSingleton<IAlbumMetadataSyncService>(p => p.GetRequiredService<AlbumMetadataSyncService>());
builder.Services.AddHostedService(p => p.GetRequiredService<AlbumMetadataSyncService>());

builder.Services.AddSingleton<TranscodingService>();
builder.Services.AddSingleton<ITranscodingService>(sp => sp.GetRequiredService<TranscodingService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<TranscodingService>());

builder.Services.AddDbContext<MusicStreamerDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("MusicStreamerDb"));
});

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
});

builder.Services.AddHttpClient("MusicBrainz", client =>
{
    client.BaseAddress = new Uri("https://musicbrainz.org/ws/2/");
    client.DefaultRequestHeaders.Add("User-Agent", $"MusicStreamerBackend/0.1 ( office@rpopovici.co.uk )");
});
builder.Services.AddHttpClient("CoverArtArchive", client =>
{
    client.BaseAddress = new Uri("https://coverartarchive.org/");
    client.DefaultRequestHeaders.Add("User-Agent", $"MusicStreamerBackend/0.1 ( office@rpopovici.co.uk )");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSvelteKit", policy =>
    {
        policy.AllowAnyOrigin() // Allow all origins
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddRequestTimeouts(options =>
{
    options.AddPolicy("LongRunning", TimeSpan.FromSeconds(60));
});

var app = builder.Build();
app.UseCors("AllowSvelteKit");

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

if (!string.IsNullOrEmpty(builder.Configuration["CoverArtFolder"]) && !string.IsNullOrEmpty(builder.Configuration["CoverArtRootPath"]))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(builder.Configuration["CoverArtFolder"]!),
        RequestPath = builder.Configuration["CoverArtRootPath"],
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
        }
    });    
}

app.UseAuthorization();
app.MapControllers();
app.Run();