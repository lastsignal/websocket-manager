using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace SocketClient
{
    // ReSharper disable once ClassNeverInstantiated.Global : Program is the entry point of the app
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .MinimumLevel.Debug()
                .WriteToConsole()
                .CreateLogger();

            var host = CreateHostBuilder(args).Build();

            Log.Logger.Information("application started at: {MachineName}", Environment.MachineName);

            host.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel();
                });

    }
}
