using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SignalRSample.Hubs;
using StonkTrader.Models.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var hubConext = host.Services.GetService(typeof(IHubContext<GameHub>));
            GameInstance.SetHubContext(hubConext);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://pcjordan:44335", "https://pcjordan:44335", "http://pcjordan:24458", "https://pcjordan:24458");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
