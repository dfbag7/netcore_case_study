using System.Diagnostics;
using System.Reflection;

using FluentMigrator.Runner;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NLog;
using NLog.Extensions.Logging;

using ScanApp.Data;
using ScanApp.Migrations;
using ScanApp.Models;

namespace ScanApp
{
    internal class Program
    {
        private static void logInitialInfo(Logger logger)
        {
            var str = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "ScanApp";
            var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            str += string.IsNullOrEmpty(version) ? " (no version info)" : $" v{version}";

            var description = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "!!! DEBUG BUILD - NOT FOR PRODUCTION USAGE !!!";
            str += ": " + description;

            logger.Info(str);
        }

        private static IServiceProvider BuildDIContainer(IConfiguration config)
        {
            var appConfig = config.Get<Config>();

            return new ServiceCollection()
                .AddTransient(p =>
                {
                    return appConfig;
                })
                .AddFluentMigratorCore()
                .ConfigureRunner(migrationRunnerBuilder =>
                {
                    migrationRunnerBuilder
                        .AddSQLite()
                        .WithGlobalConnectionString(appConfig.ConnectionString)
                        .ScanIn(typeof(Migration_20220531_1230).Assembly).For.Migrations();
                })
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    loggingBuilder.AddNLog(config);
                    loggingBuilder.AddFluentMigratorConsole();
                })
                .AddScoped<IHashesRepository, HashesRepository>()
                .AddTransient<Runner>()
                .BuildServiceProvider();
        }

        private static void UpgradeDatabase(IServiceProvider serviceProvider)
        {
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine(@"Usage: ScanApp <path>");
                return 1;
            }

            var logger = LogManager.GetCurrentClassLogger();

            logInitialInfo(logger);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            int numberOfFiles = 0;

            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build();

                var serviceProvider = BuildDIContainer(config);
                using (serviceProvider as IDisposable)
                {
                    // Put the database upgrade code into a scope
                    // to ensure that all resources will be disposed correctly.
                    using (var scope = serviceProvider.CreateScope())
                    {
                        UpgradeDatabase(scope.ServiceProvider);
                    }

                    var runner = serviceProvider.GetRequiredService<Runner>();
                    
                    numberOfFiles = runner.Run(args[0]);

                    return 0;
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Exception catched");
                throw;
            }
            finally
            {
                stopwatch.Stop();

                logger.Info($"Finished. Successfuly processed {numberOfFiles} file(s) in {stopwatch.Elapsed} hours");
                LogManager.Shutdown();
            }
        }
    }
}