using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ocelot.DependencyInjection;
using Swisschain.Sdk.Server.Common;
using Swisschain.Sdk.Server.Logging;

namespace Api
{
    public class Program
    {
        private sealed class RemoteSettingsConfig
        {
            public IReadOnlyCollection<string> RemoteSettingsUrls { get; set; }
        }

        private static ILogger<Program> _logger;

        public static void Main(string[] args)
        {
            Console.Title = "Exchange Api";

            var remoteSettingsConfig = ApplicationEnvironment.Config.Get<RemoteSettingsConfig>();

            using var loggerFactory = LogConfigurator.Configure(
                "Exchange",
                ApplicationEnvironment.Config["SeqUrl"],
                remoteSettingsConfig.RemoteSettingsUrls ?? Array.Empty<string>());

            _logger = loggerFactory.CreateLogger<Program>();

            try
            {
                _logger.LogInformation("Application is being started");

                CreateHostBuilder(loggerFactory, remoteSettingsConfig).Build().Run();

                _logger.LogInformation("Application has been stopped");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Application has been terminated unexpectedly");
            }
        }

        private static IHostBuilder CreateHostBuilder(ILoggerFactory loggerFactory, RemoteSettingsConfig remoteSettingsConfig) =>
            new HostBuilder()
                .SwisschainService<Startup>(options =>
                {
                    options.UseLoggerFactory(loggerFactory);

                    var urls = remoteSettingsConfig?.RemoteSettingsUrls ?? new[] { ApplicationEnvironment.Config["RemoteSettingsUrl"] };
                    options.AddWebJsonConfigurationSources(urls);
                    
                }, w =>
                {

                }, (context, c) =>
                {
                    
                    
                })
                .ConfigureWebHost(builder =>
                    {
                        builder.ConfigureAppConfiguration((context, configurationBuilder) =>
                        {
                            LoadOcelotFile("ocelot.general", context);
                            LoadOcelotFile("ocelot.assets", context);
                            LoadOcelotFile("ocelot.operations", context);
                            LoadOcelotFile("ocelot.order-book", context);
                            LoadOcelotFile("ocelot.account-data", context);
                            LoadOcelotFile("ocelot.temp", context);

                            configurationBuilder.AddOcelot(context.HostingEnvironment);
                        });
                    });

        private static void LoadOcelotFile(string name,
            WebHostBuilderContext context)
        {
            var url = "";
            try
            {
                var remoteOcelotHost = ApplicationEnvironment.Config["RemoteOcelotConfigHost"];
                if (!string.IsNullOrEmpty(remoteOcelotHost))
                {
                    if (remoteOcelotHost.Last() != '/')
                        remoteOcelotHost += "/";

                    using var client = new HttpClient();

                    url = $"{remoteOcelotHost}{name}";
                    var json = client.GetStringAsync(url).Result;

                    var rootPath = context.HostingEnvironment.ContentRootPath;
                    var fileName = $"{name}.json";

                    using var writer = new StreamWriter(Path.Combine(rootPath, fileName), false);
                    writer.WriteLine(json);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on load ocelot: {ocelotUrl}.", url);
            }

        }
    }
}
