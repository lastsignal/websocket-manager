using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

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

            services.AddHostedService<WebSocketClientHostedService1<TMessageHandler>>();

            services.AddSingleton<ConnectionManager>();
            services.AddSingleton<IWebSocketClientService, WebSocketClientClientService<TMessageHandler>>();

            services.Configure(options);
        }

        public static void AddWebSocketClient<TMessageHandler>(this IServiceCollection services,
            IConfiguration configuration, string configurationSection)
            where TMessageHandler : class, IWebSocketReceivingMessageHandler
        {

            var serverConfiguration = configuration
                .GetSection(configurationSection)
                .Get<IEnumerable<Endpoint>>();

            services.AddSingleton<IWebSocketReceivingMessageHandler, TMessageHandler>();

            AddHostedServicesForEachServerEndpoint<TMessageHandler>(services, serverConfiguration);

            services.AddSingleton<ConnectionManager>();
            services.AddSingleton<IWebSocketClientService, WebSocketClientClientService<TMessageHandler>>();

            services.Configure<WebSocketClientConfiguration>(clientConfiguration =>
                configuration.GetSection(configurationSection).Bind(clientConfiguration));
        }

        private static void AddHostedServicesForEachServerEndpoint<TMessageHandler>(IServiceCollection services, IEnumerable<Endpoint> serverConfiguration)
            where TMessageHandler : class, IWebSocketReceivingMessageHandler
        {

            /*
             * The current version of dotnet doesn't support AddHostedService more than once for the same type.
             * https://github.com/dotnet/runtime/issues/38751
             *
             * When the issue fixed, the following funky code can be replaced by a simple for loop. Until then,
             * we take limited number of hosts. 
             */


            // --- first endpoint ------------------------------------------------------------------------------
            var connectingServers = serverConfiguration.ToList();

            var clientConfiguration1 = connectingServers.FirstOrDefault();

            if (clientConfiguration1 == null)
                return;

            services.AddHostedService(provider => new WebSocketClientHostedService1<TMessageHandler>(
                clientConfiguration1,
                provider.GetService<ILogger>(),
                provider.GetService<IWebSocketClientService>()));

            // --- second endpoint -----------------------------------------------------------------------------
            var clientConfiguration2 = connectingServers.Skip(1).FirstOrDefault();

            if (clientConfiguration2 == null)
                return;

            services.AddHostedService(provider => new WebSocketClientHostedService2<TMessageHandler>(
                clientConfiguration2,
                provider.GetService<ILogger>(),
                provider.GetService<IWebSocketClientService>()));

            // --- add third and more if required --------------------------------------------------------------
        }
    }
}
