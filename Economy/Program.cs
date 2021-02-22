using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace Economy {
    class Program {
        static async Task Main(string[] args) {
            DotNetEnv.Env.Load();
            var discord = new DiscordClient(new DiscordConfiguration() {
                Token = Environment.GetEnvironmentVariable("BOT_TOKEN"),
                TokenType = TokenType.Bot
            });
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration() {
                StringPrefixes = new [] { "!", "eco!" }
            });
            commands.RegisterCommands<EconomyCommands>();
            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}