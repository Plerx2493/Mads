using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using MADS.Extensions;

namespace MADS.Commands.Slash;

public class About : MadsBaseApplicationCommand
{
    [SlashCommand("about", "Infos about the bot")]
    public async Task AboutCommand(InteractionContext ctx)
    {
        var discordEmbedBuilder = CommandUtility.GetDiscordEmbed();
        var discordMessageBuilder = new DiscordInteractionResponseBuilder();
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
        discordMessageBuilder.AsEphemeral();

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, discordMessageBuilder);
    }
}