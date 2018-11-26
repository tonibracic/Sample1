using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Ordering.SignalrHub
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
