using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RedditImageBot.Database;
using RedditImageBot.Services.Abstractions;
using RedditImageBot.Services.ImageProcessors;
using RedditImageBot.Services.WebAgents;
using RedditImageBot.Utilities;
using RedditImageBot.Utilities.Common;
using RedditImageBot.Utilities.Configurations;
using Serilog;

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
            services.Configure<BotInformationConfiguration>(configuration.GetSection(nameof(BotInformationConfiguration)));

            services.AddSingleton<RedditWebAgent>();
            services.AddScoped<IRedditService, RedditService>();
            services.AddScoped<IImgurService, ImgurService>();
            services.AddScoped<IImageService, ImageService>();

            services.AddScoped<FramelessImageProcessor>();
            services.AddScoped<GifImageProcessor>();
            services.AddScoped<IImageProcessorFactory, ImageProcessorFactory>();
        }

        public static void ConfigureTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();

                    var serilogLogger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext()
                    .CreateLogger();

                    loggingBuilder.AddOpenTelemetry(options =>
                    {
                        options.AttachLogsToActivityEvent();
                        options.IncludeScopes = true;
                        options.IncludeFormattedMessage = true;
                    });
                    loggingBuilder.AddSerilog(logger: serilogLogger, dispose: true);
                });

            var otlpExporterConfiguration = configuration.GetOtlpExporterConfiguration();

            services
                 .AddOpenTelemetry()
                 .WithTracing(tracerProviderBuilder =>
                 {
                     tracerProviderBuilder
                        .AddSource(ActivitySources.RedditImageBot.Name)
                        .ConfigureResource(resource => resource
                            .AddService(ActivitySources.RedditImageBot.Name))
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddOtlpExporter();
                 })
                 .WithMetrics(metricsProviderBuilder =>
                 {
                     metricsProviderBuilder
                        .ConfigureResource(resource => resource
                            .AddService(ActivitySources.RedditImageBot.Name))
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddProcessInstrumentation()
                        .AddOtlpExporter();
                 });
        }

        private static OtlpExporterConfiguration GetOtlpExporterConfiguration(this IConfiguration configuration)
        {
            var otlpExporterConfiguration = new OtlpExporterConfiguration();
            configuration.Bind(nameof(OtlpExporterConfiguration), otlpExporterConfiguration);
            return otlpExporterConfiguration;
        }
    }
}
