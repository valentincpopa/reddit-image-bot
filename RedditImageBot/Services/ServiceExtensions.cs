using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RedditImageBot.Database;
using RedditImageBot.Models;
using RedditImageBot.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RedditImageBot.Services
{
    public static class ServiceExtensions
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();
            services.AddDbContext<ApplicationDbContext>(x => x.UseNpgsql(configuration.GetConnectionString("Heroku"), options =>
            {
                options.MigrationsAssembly(typeof(ServiceExtensions).Assembly.GetName().Name);
            }));
            services.Configure<RedditConfiguration>(configuration.GetSection("RedditConfiguration"));
            services.Configure<ImgurConfiguration>(configuration.GetSection("ImgurConfiguration"));
            services.Configure<ImageConfiguration>(configuration.GetSection("ImageConfiguration"));
            services.AddSingleton<RedditWebAgent>();
            services.AddScoped<IRedditService, RedditService>();
            services.AddScoped<IImgurService, ImgurService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IBotService, BotService>();
        }
    }
}
