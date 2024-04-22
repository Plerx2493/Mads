// Copyright 2023 Plerx2493
//
// Licensed under the Apache License, Version 2.0 (the "License")
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MADS.CustomComponents;
using MADS.Entities;
using MADS.Extensions;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    static EventListener()
    {
        //retrieves the config.json
        MadsConfig config = DataProvider.GetConfig();
        
        //Create a discordWebhookClient and add the debug webhook from the config.json
        WebhookClient = new DiscordWebhookClient();
        Uri webhookUrl = new(config.DiscordWebhook);
        WebhookClient.AddWebhookAsync(webhookUrl).GetAwaiter().GetResult();
    }
    
    internal static DiscordWebhookClient WebhookClient;
    
    internal static async Task DmHandler(DiscordClient client, MessageCreateEventArgs e)
    {
        if (!e.Channel.IsPrivate)
        {
            return;
        }

        if (e.Author.IsBot)
        {
            return;
        }

        if (client.CurrentApplication.Owners?.Contains(e.Author) ?? false)
        {
            return;
        }

        if (client.CurrentApplication.Team?.Members.Any(x => x.User.Id == e.Author.Id) ?? false)
        {
            return;
        }

        if (e.Message.Content is null)
        {
            return;
        }

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithAuthor("Mads-DMs")
            .WithColor(new DiscordColor(0, 255, 194))
            .WithTimestamp(DateTime.UtcNow)
            .WithTitle($"Dm from {e.Author.Username}")
            .WithDescription(e.Message.Content);

        DiscordButtonComponent button =
            new DiscordButtonComponent(DiscordButtonStyle.Success, "Placeholder", "Respond to User").AsActionButton(
                ActionDiscordButtonEnum.AnswerDmChannel, e.Channel.Id);

        DiscordChannel channel = await client.GetChannelAsync(WebhookClient.Webhooks[0].ChannelId);
        await channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(embed).AddComponents(button));
    }
}