using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Hubs;
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
         GameManager.SetHubContext(hubConext);
         host.Run();
      }

      public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
              .ConfigureWebHostDefaults(webBuilder =>
              {
                 webBuilder.UseStartup<Startup>();
                 webBuilder.UseUrls("http://localhost:5000", "http://10.0.0.210:5000", "http://PCJORDAN:5000");
              });
   }
}
