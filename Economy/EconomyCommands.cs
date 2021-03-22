using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
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
        [Description("Prints the amount of money someone has, defaults to how much you have")]
        public async Task MoneyCommand(CommandContext ctx,
            [Description("The person to get the amount of money from")] DiscordMember user = null) {
            user ??= ctx.Member;
            var name = user.DisplayName;
            if (user == ctx.Member) {
                name = "You";
            }
            await ctx.RespondAsync($"{name} ha{(name == "You" ? "ve" : "s")} {(await GetOrCreateUser(user.Id.ToString())).Coins} coins");
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
        [Description("browse the shop for items")]
        public async Task ShopCommand(CommandContext ctx,
            [Description("The query to search for")] string query = "") {
            Console.WriteLine("Shop command run");
            var items = await _helper.GetItems(query);
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
        
        
        [Command("buy")]
        [Description("Buys an item from the shop. The query must match only one item.")]
        public async Task BuyCommand(CommandContext ctx,
            [Description("The item to buy from the shop")] string item,
            [Description("The number of items to buy")] int count = 1) {    
            var user = await GetOrCreateUser(ctx.User.Id.ToString());
            var itemsMatched = await _helper.GetItems(item);
            if (itemsMatched.Count == 0) {
                await ctx.RespondAsync("Could not find that item");
                return;
            } else if (itemsMatched.Count > 1) {
                await ctx.RespondAsync(
                    "That search would give you more that one item, I don't know which to give you!");
                return;
            }

            var itemToBuy = itemsMatched[0];
            if (user.Coins < itemToBuy.BuyPrice * count) {
                await ctx.RespondAsync("You don't have enough money to buy those");
                return;
            }

            user.Coins -= itemToBuy.BuyPrice * count;
            user.Items ??= new List<ItemRef>();

            foreach (var userItem in user.Items) {
                if (userItem.Ref.Id.Equals(itemToBuy.Id)) {
                    userItem.Count += count;
                    break;
                }
            }

            await _helper.UpdateUser(user.Id, user);
            await ctx.RespondAsync($"You bought {count} {itemToBuy.Name}");
        }

        [Command("sell")]
        [Description("Sells. The query must match only one item.")]
        public async Task SellCommand(CommandContext ctx,
            [Description("The item to sell")] string item,
            [Description("The number of items to sell")] int count = 1) {    
            var user = await GetOrCreateUser(ctx.User.Id.ToString());
            var itemsMatched = await _helper.GetItems(item);
            if (itemsMatched.Count == 0) {
                await ctx.RespondAsync("Could not find that item");
                return;
            } else if (itemsMatched.Count > 1) {
                await ctx.RespondAsync(
                    "That search would give you more that one item, I don't know which to give you!");
                return;
            }

            user.Items ??= new List<ItemRef>();
            ItemRef itemSold = null;
            foreach (var userItem in user.Items) {
                if (userItem.Ref.Id.Equals(itemsMatched[0].Id)) {
                    itemSold = userItem;
                    break;
                }
            }

            if (itemSold.Count < count) {
                await ctx.RespondAsync("You dont have enough of that item to sell");
                return;
            }

            itemSold.Count -= count;
            user.Coins += itemsMatched[0].SellPrice * count;

            await _helper.UpdateUser(user.Id, user);
            await ctx.RespondAsync($"You sold {count} {(await _helper.GetItem(itemSold.Ref.Id.ToString())).Name}");
        }

        [Command("inv")]
        [Description("Gets someone's inventory")]
        public async Task InvCommand(CommandContext ctx,
            [Description("The user to get the inventory of. Defaults to you")] DiscordMember member = null
        ) {
            member ??= ctx.Member;
            StringBuilder builder = new StringBuilder($"{member.DisplayName}'s inventory:\n");
            var user = await GetOrCreateUser(member.Id.ToString());
            foreach (var itemRef in user.Items) {
                var item = await _helper.GetItem(itemRef.Ref.Id.ToString());
                var emoji = await ctx.Guild.GetEmojiAsync(UInt64.Parse(item.EmoteId));
                builder.Append($"- {itemRef.Count} {emoji.ToString()} {item.Name}\n");
            }

            await ctx.RespondAsync(builder.ToString());
        }
    }
}