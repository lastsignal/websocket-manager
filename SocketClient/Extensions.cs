using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;
using System;

namespace SocketClient
{
    public static class Extensions
    {
        private static string Env =>
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development;


        public static LoggerConfiguration WriteToConsole(this LoggerConfiguration loggerConfiguration)
        {
            if (Env == Environments.Development)
            {
                const string template =
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{Properties:j}{NewLine}{Exception}";
                loggerConfiguration.WriteTo.Console(outputTemplate: template);
            }
            else
            {
                loggerConfiguration.WriteTo.Console(new RenderedCompactJsonFormatter());
            }

            return loggerConfiguration;
        }
    }
}
