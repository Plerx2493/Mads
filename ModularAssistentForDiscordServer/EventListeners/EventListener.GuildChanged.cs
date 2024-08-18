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
using MADS.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    public static async Task OnGuildCreated(DiscordClient sender, GuildCreatedEventArgs args)
    {
        DiscordMember owner = await args.Guild.GetGuildOwnerAsync();
        
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle($"New guild added: {args.Guild.Name}")
            .AddField("Id:", args.Guild.Id.ToString())
            .AddField("Owner:", owner.GlobalName ?? owner.Username)
            .AddField("Membercount:", args.Guild.MemberCount.ToString())
            .AddField("Added:", Formatter.Timestamp(DateTimeOffset.Now))
            .AddField("Created:", Formatter.Timestamp(args.Guild.CreationTimestamp))
            .WithColor(DiscordColor.Green);
        
        await ModularDiscordBot.Services
            .GetRequiredService<LoggingService>()
            .LogToWebhook(new DiscordMessageBuilder().AddEmbed(embed));
    }
    
    public static async Task OnGuildDeleted(DiscordClient sender, GuildDeletedEventArgs args)
    {
        DiscordUser owner = await sender.GetUserAsync(args.Guild.OwnerId);
        
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle($"Guild removed: {args.Guild.Name}")
            .AddField("Id:", args.Guild.Id.ToString())
            .AddField("Owner:", owner.GlobalName)
            .AddField("Membercount:", args.Guild.MemberCount.ToString())
            .AddField("Removed:", Formatter.Timestamp(DateTimeOffset.Now))
            .AddField("Created:", Formatter.Timestamp(args.Guild.CreationTimestamp))
            .WithColor(DiscordColor.Red);
        
        await ModularDiscordBot.Services
            .GetRequiredService<LoggingService>()
            .LogToWebhook(new DiscordMessageBuilder().AddEmbed(embed));
    }
}