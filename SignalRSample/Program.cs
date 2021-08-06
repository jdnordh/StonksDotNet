using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StonkTrader.Models.Workers;

namespace SignalRSample
{
	public class Program
	{
		public static void Main(string[] args)
		{
			IHost host = CreateHostBuilder(args).Build();
			host.Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
						.ConfigureServices((hostContext, services) => 
						{
							services.AddHostedService<GameWorker>();
						})
						.ConfigureWebHostDefaults(webBuilder => {
							webBuilder.UseStartup<Startup>();
							webBuilder.UseUrls("http://localhost:5000", "http://10.0.0.210:5000", "http://PCJORDAN:5000");
						});
		}
	}
}
