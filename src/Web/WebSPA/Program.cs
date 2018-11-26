using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using Serilog.Events;

namespace eShopConContainers.WebSPA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
             .UseStartup<Startup>()
                .UseHealthChecks("/hc")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .UseApplicationInsights()
                .UseSerilog((builderContext, config) =>
                {
                    config
                        .MinimumLevel.Debug()
                        .Enrich.FromLogContext();

                    if (builderContext.Configuration["UseElasticSearch"] == Boolean.TrueString)
                    {
                        config.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builderContext.Configuration["ElasticSearchSvcUrl"]))
                        {
                            MinimumLogEventLevel = LogEventLevel.Verbose,
                            AutoRegisterTemplate = true
                        })
                        .CreateLogger();
                    }
                    else
                    {
                        config.WriteTo.Console();
                    }
                })
                .Build();       
    }
}
