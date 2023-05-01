using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RedditImageBot.Database;
using RedditImageBot.Services;

namespace RedditImageBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            }

            host.Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.ConfigureTelemetry(hostContext.Configuration);
                services.ConfigureServices(hostContext.Configuration);
                services.AddHostedService<BotProcessingService>();
                services.AddHostedService<BotMessageRescheduler>();
                services.AddAutoMapper(typeof(Program).Assembly);
            });
    }
}
