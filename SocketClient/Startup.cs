using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WebSocketManager;

namespace SocketClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton(Log.Logger);


            // >> Register WebSocketClient
            services.AddWebSocketClient<WebSocketMessageHandler>(options =>
            {
                options.ServerEndpoints = new[]
                {
                    "wss://localhost:5010/ws",
                    "wss://localhost:5011/ws"
                };
                options.RetryConnectInSeconds = 15;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // >> Use WebSocketClient
            app.UseWebSocketClient();
        }
    }
}
