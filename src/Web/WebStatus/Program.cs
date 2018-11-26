using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;
using System.IO;

namespace WebStatus
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
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
