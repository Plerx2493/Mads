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

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using MADS.Entities;
using Microsoft.Extensions.Logging;

namespace MADS.Services;

public class AntiPhishingService
{
    private HttpClient antiFishClient;
    private HttpClient pishggClient;

    public ILogger Logger { get; set; }

    private Regex _linkRegex = new("(?:[A-z0-9](?:[A-z0-9-]{0,61}[A-z0-9])?\\.)+[A-z0-9][A-z0-9-]{0,61}[A-z0-9]",
        RegexOptions.Compiled);

    private JsonSerializerOptions tmp = new JsonSerializerOptions()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        
    };

    public AntiPhishingService(DiscordClient discordClient)
    {
        Logger = discordClient.Logger;

        var httpHandler = new HttpClientHandler
        {
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
        };

        antiFishClient = new HttpClient(httpHandler)
        {
            BaseAddress = new Uri("https://anti-fish.bitflow.dev/check")
        };

        antiFishClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
            "MadsDiscordBot (https://github.com/Plerx2493/Mads, v1)");
        
        

        pishggClient = new HttpClient(httpHandler)
        {
            BaseAddress = new Uri("https://api.phish.gg/")
        };

        pishggClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
            "MadsDiscordBot (https://github.com/Plerx2493/Mads, v1)");
        


        discordClient.MessageCreated += HandleMessage;
        discordClient.MessageUpdated += HandleMessageUpdate;
    }


    private async Task HandleMessageUpdate(DiscordClient sender, MessageUpdateEventArgs args)
    {
        var _ = Task.Run(() => CheckLinksAsync(args.Message));
    }

    private async Task HandleMessage(DiscordClient sender, MessageCreateEventArgs args)
    {
        var _ = Task.Run(() => CheckLinksAsync(args.Message));
    }

    private async Task<AntiFishResponse?> CheckLinksAsync(DiscordMessage message)
    {
        if (message.Content == null) return null;
        if (message.Author.IsBot) return null;
        if (message.WebhookMessage) return null;

        var matches = _linkRegex.Matches(message.Content);

        if (matches.Count == 0) return null;

        Logger.LogDebug("Link will be checked");

        var json = new JsonObject();
        json.Add("message", message.Content);

        var res = await antiFishClient.PostAsJsonAsync("", json);

        if (res.StatusCode == HttpStatusCode.NotFound) return null;
        
        if (res.StatusCode != HttpStatusCode.OK)
        {
            Logger.LogError($"AntiPhishing failed: {res.StatusCode.Humanize()} {res.ReasonPhrase}");
            return null;
        }

        return await res.Content.ReadFromJsonAsync<AntiFishResponse>();
    }

    private async Task<PhishggResponse?> CheckUsernameAsync(string username)
    {
        var json = new JsonObject();
        json.Add("username", username);

        var res = await antiFishClient.PostAsJsonAsync("username", json);
        
        if (res.StatusCode != HttpStatusCode.OK)
        {
            Logger.LogError($"AntiPhishing failed: {res.StatusCode.Humanize()} {res.ReasonPhrase}");
            return null;
        }
        
        return await res.Content.ReadFromJsonAsync<PhishggResponse>();
    }

    private async Task<PhishggResponse?> CheckServerAsync(ulong serverId)
    {
        var res = await antiFishClient.GetAsync($"server?id={serverId}");
        
        if (res.StatusCode != HttpStatusCode.OK)
        {
            Logger.LogError($"AntiPhishing failed: {res.StatusCode.Humanize()} {res.ReasonPhrase}");
            return null;
        }
        
        return await res.Content.ReadFromJsonAsync<PhishggResponse>();
    }
}