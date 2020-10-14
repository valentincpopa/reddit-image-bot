using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RedditImageBot.Services;
using System;

namespace RedditImageBot
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<HostedService>();                
            });
    }
}
