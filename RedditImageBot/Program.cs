using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RedditImageBot.Services;

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
                services.ConfigureTelemetry(hostContext.Configuration);
                services.ConfigureServices(hostContext.Configuration);
                services.AddHostedService<HostedService>();
                services.AddAutoMapper(typeof(Program).Assembly);
            });
    }
}
