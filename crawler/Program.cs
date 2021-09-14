using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace prnt_sc_crawler
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //new ConfigurationBuilder().AddCommandLine(args).Build();
            await Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<PrntScCrawlerHostedService>();
                })
                .RunConsoleAsync();
        }
    }
}
