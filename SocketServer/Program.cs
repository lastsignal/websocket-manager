using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using WebSocketManager;
using SocketServer;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .MinimumLevel.Debug()
    .WriteToConsole()
    .CreateLogger();

Log.Logger.Information("Server application started at: {MachineName}", Environment.MachineName);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton(Log.Logger);

// Add WebSocketManager Server
builder.Services.AddWebSocketServer<ServerSideWebSocketReceivingMessageHandler>();


var app = builder.Build();
app.UseRouting();
app.UseAuthorization();

// Use WebSocketManager Server; needs to be placed before UseEndpoints!
app.UseWebSocketServer("/ws");

app.MapControllers();

await app.RunAsync();
