using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;

namespace Ordering.BackgroundTasks
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
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    builder.AddDebug();
                    builder.AddConsole();
                })
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
