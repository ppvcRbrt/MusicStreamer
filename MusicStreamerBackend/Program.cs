var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
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