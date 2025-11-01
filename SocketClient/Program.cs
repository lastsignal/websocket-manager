using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SocketClient;
using System;
using WebSocketManager;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .MinimumLevel.Debug()
    .WriteToConsole()
    .CreateLogger();

Log.Logger.Information("Client application started at: {MachineName}", Environment.MachineName);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton(Log.Logger);

// Register WebSocketClient
builder.Services.AddWebSocketClient<WebSocketMessageHandler>(options =>
{
    options.ServerEndpoints = [
        "wss://localhost:5010/ws",
        // in case more than one server is running
        // "wss://localhost:5011/ws",
        ];
});

var app = builder.Build();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Use WebSocketClient
app.UseWebSocketClient();

await app.RunAsync();
