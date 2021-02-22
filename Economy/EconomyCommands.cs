using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Economy.Models;
using MongoDB.Driver;

namespace Economy {
    public class EconomyCommands : BaseCommandModule {

        private DatabaseHelper _helper = new DatabaseHelper();
        
        [Command("money")]
        public async Task MoneyCommand(CommandContext ctx) {
            if (await _helper.GetUser(ctx.User.Id.ToString()) == null) {
                await _helper.Create(new User(ctx.User.Id.ToString()));
            }

            await ctx.RespondAsync($"You have {(await _helper.GetUser(ctx.User.Id.ToString())).Coins} coins");
        }
    }
}