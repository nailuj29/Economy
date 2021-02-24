using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Economy.Models;
using MongoDB.Driver;

namespace Economy {
    public class EconomyCommands : BaseCommandModule {

        private DatabaseHelper _helper = new DatabaseHelper();

        private async Task<User> GetOrCreateUser(string id) {
            if (await _helper.GetUser(id) == null) {
                return await _helper.CreateUser(new User(id));
            }

            return await _helper.GetUser(id);
        }

        [Command("money")]
        [Description("Prints the amount of money you have")]
        public async Task MoneyCommand(CommandContext ctx) {
            await ctx.RespondAsync($"You have {(await GetOrCreateUser(ctx.User.Id.ToString())).Coins} coins");
        }

        [Command("give")]
        [Description("Gives someone money")]
        public async Task GiveCommand(CommandContext ctx, 
            [Description("The person to give money to")] DiscordMember to,
            [Description("The amount to give")] long amount) {
            var sender = await GetOrCreateUser(ctx.User.Id.ToString());
            var recipient = await GetOrCreateUser(to.Id.ToString());

            if (sender.Coins < amount) {
                await ctx.RespondAsync($"You dont have enough money to give to {to.Nickname}");
                return;
            }
            
            sender.Coins -= amount;
            recipient.Coins += amount;

            await _helper.UpdateUser(sender.Id, sender);
            await _helper.UpdateUser(recipient.Id, recipient);

            await ctx.RespondAsync(
                $"You sent {amount} coins to {to.Nickname}. \nYou now have {sender.Coins} coins and they have {recipient.Coins}");
        }
        
        // TODO: Remove this once testing is complete
        [Command("freemoney")]
        public async Task FreeMoneyCommand(CommandContext ctx) {
            var recipient = await GetOrCreateUser(ctx.User.Id.ToString());
            recipient.Coins += 1_000_000_000;
            await _helper.UpdateUser(recipient.Id, recipient);

            await ctx.RespondAsync("Here's free money");
        }


        [Command("shop")]
        public async Task ShopCommand(CommandContext ctx) {
            Console.WriteLine("Shop command run");
            var items = await _helper.GetItems();
            var pages = new List<Page>();
            if (items is null || items.Count == 0) {
                await ctx.RespondAsync("The shop could not be loaded.");
                return;
            }
            foreach (var item in items) {
                var builder = new DiscordEmbedBuilder() {
                    Title = item.Name,
                    Description = item.Description,
                    ImageUrl = $"https://cdn.discordapp.com/emojis/{item.EmoteId}"
                };
                builder.AddField("Buy", item.BuyPrice.ToString());
                builder.AddField("Sell", item.SellPrice.ToString());
                pages.Add(new Page(embed: builder));
            }

            await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages);
        }
    }
}