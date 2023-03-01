using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMethodReturnValue.Global : usage of return value is optional to the consuming app

namespace WebSocketManager
{
    public static class Extensions
    {
        public static string GetMachineName()
        {
            var machineName = Environment.MachineName;
            if (string.IsNullOrWhiteSpace(machineName))
                machineName = Environment.GetEnvironmentVariable("COMPUTERNAME");

            if (string.IsNullOrWhiteSpace(machineName))
                machineName = Environment.GetEnvironmentVariable("HOSTNAME");

            if (string.IsNullOrWhiteSpace(machineName))
                machineName =
                    $"{Environment.GetEnvironmentVariable("CF_INSTANCE_INDEX")};{Environment.GetEnvironmentVariable("CF_INSTANCE_ADDR")}";

            return machineName;
        }

        private static void AddWebSocketManager(this IServiceCollection services)
        {
            services.AddSingleton<ConnectionManager>();
            services.AddSingleton<WebSocketServerService>();
            services.AddSingleton<IWebSocketServerService, WebSocketServerService>();
        }

        private static IApplicationBuilder PrivateUseWebSocket(this IApplicationBuilder app, WebSocketOptions options = null)
        {
            if (options == null)
                app.UseWebSockets();
            else
                app.UseWebSockets(options);

            return app;
        }

        public static IApplicationBuilder UseWebSocketClient(this IApplicationBuilder app, WebSocketOptions options = null)
        {
            return app.PrivateUseWebSocket(options);
        }

        public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder app, PathString path, WebSocketOptions options = null)
        {
            app.PrivateUseWebSocket(options);
            return app.Map(path, builder => builder.UseMiddleware<WebSocketServerMiddleware>());
        }

        public static IServiceCollection AddWebSocketServer<TWebSocketServerSideHandler>(this IServiceCollection services)
            where TWebSocketServerSideHandler : class, IWebSocketReceivingMessageHandler
        {
            services.AddSingleton<IWebSocketReceivingMessageHandler, TWebSocketServerSideHandler>();
            services.AddWebSocketManager();

            return services;
        }

        public static void AddWebSocketClient<TMessageHandler>(this IServiceCollection services, Action<WebSocketClientConfiguration> options)
            where TMessageHandler : class, IWebSocketReceivingMessageHandler
        {
            services.AddSingleton<IWebSocketReceivingMessageHandler, TMessageHandler>();

            services.AddHostedService<WebSocketClientHostedService<TMessageHandler>>();

            services.AddSingleton<ConnectionManager>();
            services.AddSingleton<IWebSocketClientService, WebSocketClientClientService<TMessageHandler>>();

            services.Configure(options);
        }
    }
}
