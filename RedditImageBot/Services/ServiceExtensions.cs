using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedditImageBot.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedditImageBot.Services
{
    public static class ServiceExtensions
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();
            services.Configure<RedditConfiguration>(configuration.GetSection("RedditConfiguration"));
            services.AddSingleton<RedditWebAgent>();
        }
    }
}
