using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotProject.Utilities;
using Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotProject.Modules
{
    public class General : ModuleBase
    {
        private readonly ILogger<General> _logger;      
        private readonly Images _images;
        private readonly RanksHelper _ranksHelper;

        public General( ILogger<General> logger, Images image, RanksHelper ranksHelper)
        {
            _logger = logger;
            _images = image;
            _ranksHelper = ranksHelper;
        }
        //[Command("help")]
        //public async Task Help()
        //{
        //    EmbedBuilder embedBuilder = new EmbedBuilder();

        //    foreach (CommandInfo command in commands)
        //    {
        //        // Get the command Summary attribute information
        //        string embedFieldText = command.Summary ?? "No description available\n";

        //        embedBuilder.AddField(command.Name, embedFieldText);
        //    }

        //    await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        //}

        [Command("image", RunMode = RunMode.Async)]
        public async Task Image(SocketGuildUser user)
        {
            var path = await _images.CreateImageAsync(user);
            await Context.Channel.SendFileAsync(path);
            File.Delete(path);
        }

        [Command("ping")]
        [Summary("ping pong")]
        public async Task Ping()
        {
            await Context.Channel.SendMessageAsync($"pong!");
            _logger.LogInformation($"{Context.User.Username} executed the PING command");
        }

        [Command("echo")]
        public async Task EchoAsync([Remainder] string text)
        {
            await ReplyAsync(text);
            _logger.LogInformation($"{Context.User.Username} executed the ECHO command");
        }

        [Command("info")]
        [Summary("информация об аккаунте")]
        public async Task Info([Summary("Имя пользователя")] SocketGuildUser user = null)
        {
            if (user == null)
            {
                var builder = new EmbedBuilder()
                    .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                    .WithDescription("Information message about yourself!")
                    .WithColor(new Color(33, 231, 14))
                    .AddField("User ID", Context.User.Id, true)
                    .AddField("Discriminator", Context.User.Discriminator, true)
                    .AddField("Created at:", Context.User.CreatedAt.ToString("dd/MM/yyyy"))
                    .AddField("Joined at:", (Context.User as SocketGuildUser).JoinedAt.Value.ToString("dd/MM/yyyy"), true)
                    .AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                    .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
                _logger.LogInformation($"{Context.User.Username} executed the INFO command");
            }
            else
            {
                var builder = new EmbedBuilder()
                    .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                    .WithDescription($"Information message about {user.Username}!")
                    .WithColor(new Color(33, 231, 14))
                    .AddField("User ID", user.Id, true)
                    .AddField("Discriminator", user.Discriminator, true)
                    .AddField("Created at:", user.CreatedAt.ToString("dd/MM/yyyy"))
                    .AddField("Joined at:", (user as SocketGuildUser).JoinedAt.Value.ToString("dd/MM/yyyy"), true)
                    .AddField("Roles", string.Join(" ", (user as SocketGuildUser).Roles.Select(x => x.Mention)))
                    .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
                _logger.LogInformation($"{Context.User.Username} executed the INFO ({user}) command");
            }
        }

        [Command("server")]
        public async Task Server()
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("Information about this server.")
                .WithTitle($"{Context.Guild.Name} Information")
                .WithColor(Discord.Color.Teal)
                .AddField("Created at", Context.Guild.CreatedAt.ToString("dd/MM/yyyy"))
                .AddField("memberCount", (Context.Guild as SocketGuild).MemberCount + " members", true)
                .AddField("Online users", (Context.Guild as SocketGuild).Users.Where(x => x.Status == UserStatus.Offline).Count() + " members", true);
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
            _logger.LogInformation($"{Context.User.Username} executed the SERVER command");
        }
        ///не работает удаление роли с пользователя
        [Command("rank", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Rank([Remainder] string identifier)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            IRole role;

            if (ulong.TryParse(identifier, out ulong roleId))
            {
                var roleById = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
                if (roleById == null)
                {
                    await ReplyAsync("That role does not exist");
                    return;
                }

                role = roleById;
            }
            else
            {
                var roleByName = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, identifier, StringComparison.CurrentCultureIgnoreCase));
                if(roleByName == null)
                {
                    await ReplyAsync("The role doesnt exist!");
                    return;
                }
                role = roleByName;
            }
            /// Any возвращает + 
            if (ranks.Any(x => x.Id != role.Id))
            {
                
            }
            else
            {
                await ReplyAsync("That rank does not exist!");
                return;
            }

            if((Context.User as SocketGuildUser).Roles.Any(x => x.Id == role.Id))
            {
                await (Context.User as SocketGuildUser).RemoveRoleAsync(role);
                await ReplyAsync($"Succesfully removed the rank {role.Mention} from you.");
                return;
            }

            await (Context.User as SocketGuildUser).AddRoleAsync(role);
            await ReplyAsync($"Succesfully added the rank {role.Mention} to you.");
        }
    }
}
