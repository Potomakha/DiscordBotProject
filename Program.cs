using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.IO;
using Discord.WebSocket;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using DiscordBotProject.Services;
using Infrastructure;
using DiscordBotProject.Utilities;

namespace DiscordBotProject
{
    class Program
    {
        //public static async Task Main(string[] args)
        //    => await Startup.RunAsync(args);
        static async Task Main()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("_config.json", false, true)
                    .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureDiscordHost<DiscordSocketClient>((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200,
                    };
                    config.Token = context.Configuration["token"];
                })
                .UseCommandService((sontext, config) =>
                {
                    config = new CommandServiceConfig()
                    {
                        CaseSensitiveCommands = false,
                        LogLevel = LogSeverity.Verbose,
                    };
                })
                .ConfigureServices((context, services) =>
                {
                    services
                    .AddHostedService<CommandHandler>()
                    .AddDbContext<Context>()
                    .AddSingleton<Servers>()
                    .AddSingleton<Images>()
                    .AddSingleton<Ranks>()
                    .AddSingleton<AutoRoles>()
                    .AddSingleton<RanksHelper>()
                    .AddSingleton<AutoRolesHelper>();
                })
                .UseConsoleLifetime();


            var host = builder.Build();
            using(host)
            {
                await host.RunAsync();
            }
        }
    }
}
