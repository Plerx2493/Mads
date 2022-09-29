using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;

namespace MADS.Commands.Text.Base;

public class About : BaseCommandModule
{
    private MadsServiceProvider            CommandService { get; set; }
    public  IDbContextFactory<MadsContext> DbFactory      { get; set; }
    
    [Command("about"), Aliases("info"), Description("Displays a little information about this bot"),
     Cooldown(1, 30, CooldownBucketType.Channel)]
    public async Task AboutCommand(CommandContext ctx)
    {
        var discordEmbedBuilder = CommandUtility.GetDiscordEmbed();
        var discordMessageBuilder = new DiscordMessageBuilder();
        var inviteUri = ctx.Client.CurrentApplication.GenerateOAuthUri(null, Permissions.Administrator, OAuthScope.Bot,
            OAuthScope.ApplicationsCommands);
        var addMe = $"[Click here!]({inviteUri.Replace(" ", "%20")})";

        var diff = DateTime.Now - CommandService.ModularDiscordBot.StartTime;
        var date = $"{diff.Days} days {diff.Hours} hours {diff.Minutes} minutes";

        discordEmbedBuilder
            .WithTitle("About me")
            .WithDescription("A modular designed discord bot for moderation and stuff")
            .WithAuthor(ctx.Client.CurrentUser.Username, ctx.Client.CurrentUser.AvatarUrl,
                ctx.Client.CurrentUser.AvatarUrl)
            .WithColor(new DiscordColor(0, 255, 194))
            .AddField("Owner:", "[Plerx#0175](https://github.com/Plerx2493/)", true)
            .AddField("Source:", "[Github](https://github.com/Plerx2493/Mads)", true)
            .AddField("D#+ Version:", ctx.Client.VersionString)
            .AddField("Guilds", ctx.Client.Guilds.Count.ToString(), true)
            .AddField("Uptime", date.Humanize(), true)
            .AddField("Ping", $"{ctx.Client.Ping} ms", true)
            .AddField("Add me", addMe);

        discordMessageBuilder.AddEmbed(discordEmbedBuilder.Build());
        discordMessageBuilder.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, "feedback-button",
            "Feedback"));

        var response = await ctx.RespondAsync(discordMessageBuilder);
        await CommandService.ModularDiscordBot.Logging.LogCommandExecutionAsync(ctx, response);
    }
}