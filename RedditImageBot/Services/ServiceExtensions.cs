using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedditImageBot.Database;
using RedditImageBot.Services.Abstractions;
using RedditImageBot.Services.WebAgents;
using RedditImageBot.Utilities;

namespace RedditImageBot.Services
{
    public static class ServiceExtensions
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();
            services.AddDbContextFactory<ApplicationDbContext>(x => x.UseNpgsql(configuration.GetConnectionString("Postgres"), options =>
            {
                options.MigrationsAssembly(typeof(ServiceExtensions).Assembly.GetName().Name);
            }));

            services.Configure<RedditWebAgentConfiguration>(configuration.GetSection(nameof(RedditWebAgentConfiguration)));
            services.Configure<ImgurWebAgentConfiguration>(configuration.GetSection(nameof(ImgurWebAgentConfiguration)));
            services.Configure<ImageConfiguration>(configuration.GetSection(nameof(ImageConfiguration)));
            services.Configure<ThreadingConfiguration>(configuration.GetSection(nameof(ThreadingConfiguration)));

            services.AddSingleton<RedditWebAgent>();
            services.AddScoped<IRedditService, RedditService>();
            services.AddScoped<IImgurService, ImgurService>();
            services.AddScoped<IImageService, ImageService>();
        }
    }
}
