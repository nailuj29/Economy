using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Economy.Models;
using MongoDB.Driver;

namespace Economy {
    public class EconomyCommands : BaseCommandModule {

        private DatabaseHelper _helper = new DatabaseHelper();

        private async Task<User> getOrCreateUser(string id) {
            if (await _helper.GetUser(id) == null) {
                return await _helper.Create(new User(id));
            }

            return await _helper.GetUser(id);
        }

        [Command("money")]
        [Description("Prints the amount of money you have")]
        public async Task MoneyCommand(CommandContext ctx) {
            await ctx.RespondAsync($"You have {(await getOrCreateUser(ctx.User.Id.ToString())).Coins} coins");
        }

        [Command("give")]
        [Description("Gives someone money")]
        public async Task GiveCommand(CommandContext ctx, 
            [Description("The person to give money to")] DiscordMember to,
            [Description("The amount to give")] long amount) {
            var sender = await getOrCreateUser(ctx.User.Id.ToString());
            var recipient = await getOrCreateUser(to.Id.ToString());

            if (sender.Coins < amount) {
                await ctx.RespondAsync($"You dont have enough money to give to {to.Nickname}");
                return;
            }
            
            sender.Coins -= amount;
            recipient.Coins += amount;

            await _helper.Update(sender.Id, sender);
            await _helper.Update(recipient.Id, recipient);

            await ctx.RespondAsync(
                $"You sent {amount} coins to {to.Nickname}. \nYou now have {sender.Coins} coins and they have {recipient.Coins}");
        }
        
        // TODO: Remove this once testing is complete
        [Command("freemoney")]
        public async Task FreeMoneyCommand(CommandContext ctx) {
            var recipient = await getOrCreateUser(ctx.User.Id.ToString());
            recipient.Coins += 1_000_000_000;
            await _helper.Update(recipient.Id, recipient);

            await ctx.RespondAsync("Here's free money");
        }
    }
}