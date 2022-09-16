using DSharpPlus.Entities;

namespace MADS.Commands;

public class CommandUtility
{
    public static DiscordEmbedBuilder GetDiscordEmbed()
    {
        var standardEmbed = new DiscordEmbedBuilder()
        {
            Color = new Optional<DiscordColor>(new DiscordColor(0, 255, 194)),
        };
        return standardEmbed;
    }
}