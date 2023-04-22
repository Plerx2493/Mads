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
using MADS.Entities;
using MADS.JsonModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MADS.Extensions;

public static class ExtensionMethods
{
    public static IServiceCollection AddDbFactoryDebugOrRelease(this IServiceCollection serviceCollection,
        ConfigJson config)
    {
        serviceCollection.AddDbContextFactory<MadsContext>(
            options => options.UseMySql(config.ConnectionString, ServerVersion.AutoDetect(config.ConnectionString))
        );

        return serviceCollection;
    }

    public static async Task<DiscordEmbed> GetEmbedAsync(this QuoteDbEntity quote, DiscordClient client)
    {
        var quotedUser = await client.GetUserAsync(quote.QuotedUserId);
        var user = await client.GetUserAsync(quote.UserId);

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(quotedUser.Username, quotedUser.AvatarUrl, quotedUser.AvatarUrl)
            .WithTitle($"Said following {Formatter.Timestamp(quote.CreatedAt)}:")
            .WithDescription(quote.Content)
            .WithFooter("Quoted by " + user.Username);

        return embed;
    }

    public static async Task<DiscordMessageBuilder> GetMessageAsync(this ReminderDbEntity reminder,
        DiscordClient client)
    {
        var message = new DiscordMessageBuilder();
        if (reminder.MentionedMessage != 0) message.WithReply(reminder.MentionedMessage);

        var user = await client.GetUserAsync(reminder.UserId);
        var userMention = new UserMention(user);
        var embed = new DiscordEmbedBuilder();

        embed.WithTitle($"{reminder.GetTimestamp()} you wanted to be reminded:")
            .WithDescription(reminder.ReminderText)
            .WithFooter("Id: " + reminder.Id)
            .WithColor(DiscordColor.Green);

        message.WithContent($"<@!{user.Id}>");
        message.WithEmbed(embed).WithAllowedMention(userMention);

        return message;
    }

    public static string GetTimestamp(this ReminderDbEntity reminder)
    {
        var timespan = reminder.CreationTime - DateTime.UtcNow;
        return Formatter.Timestamp(timespan);
    }
}