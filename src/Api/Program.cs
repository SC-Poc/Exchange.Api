using System;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swisschain.Sdk.Server.Common;
using Swisschain.Sdk.Server.Configuration.WebJsonSettings;
using Swisschain.Sdk.Server.Loggin;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Api Api";

            using (var loggerFactory = LogConfigurator.Configure("Api", ApplicationEnvironment.Config["SeqUrl"]))
            {
                var logger = loggerFactory.CreateLogger<Program>();

                try
                {
                    logger.LogInformation("Application is being started");

                    CreateHostBuilder(loggerFactory).Build().Run();

                    logger.LogInformation("Application has been stopped");
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "Application has been terminated unexpectedly");
                }
            }
        }

        private static IHostBuilder CreateHostBuilder(ILoggerFactory loggerFactory) =>
            new HostBuilder()
                .SwisschainService<Startup>(options =>
                {
                    options.UseLoggerFactory(loggerFactory);
                    options.WithWebJsonConfigurationSource(ApplicationEnvironment.Config["RemoteSettingsUrl"]);
                })
                .ConfigureAppConfiguration(c =>
                {
                    c.AddJsonFile("ocelot.json", false, false);
                    
                    var aaa = ApplicationEnvironment.Config["OcelotSettingsUrl"];
                    Console.WriteLine(aaa);
                });
    }
}
