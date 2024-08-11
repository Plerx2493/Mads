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
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using MADS.Entities;
using Microsoft.Extensions.Logging;

namespace MADS.Services;

public partial class AntiPhishingService
{
    private readonly DiscordClient _discordClient;
    private readonly HttpClient _antiFishClient;
    private readonly HttpClient _phishggClient;

    private ILogger Logger { get; set; }

    [GeneratedRegex("(?:[A-z0-9](?:[A-z0-9-]{0,61}[A-z0-9])?\\.)+[A-z0-9][A-z0-9-]{0,61}[A-z0-9]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, 50)]
    private partial Regex LinkRegex();

    [GeneratedRegex(@"(https?:\/\/)?(.*?@)?(www\.)?(discord\.(gg)|discord(app)?\.com\/invite)\/(?<code>[\w-]+)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, 50)]
    private partial Regex InviteRegex();
    
    public AntiPhishingService(DiscordClient discordClient, ILogger<AntiPhishingService> logger)
    {
        Logger = logger;
        _discordClient = discordClient;

        HttpClientHandler httpHandler = new()
        {
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.All
        };

        _antiFishClient = new HttpClient(httpHandler)
        {
            BaseAddress = new Uri("https://anti-fish.bitflow.dev/check")
        };

        _antiFishClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
            "MadsDiscordBot (https://github.com/Plerx2493/Mads, v1)");
        
        
        _phishggClient = new HttpClient(httpHandler)
        {
            BaseAddress = new Uri("https://api.phish.gg/")
        };

        _phishggClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
            "MadsDiscordBot (https://github.com/Plerx2493/Mads, v1)");
        
        
        discordClient.MessageCreated += HandleMessage;
        discordClient.MessageUpdated += HandleMessageUpdate;
    }
    
    private Task HandleMessageUpdate(DiscordClient sender, MessageUpdatedEventArgs args)
    {
        return CheckLinksAsync(args.Message);
    }

    private async Task HandleMessage(DiscordClient sender, MessageCreatedEventArgs args)
    {
        AntiFishResponse? links = await CheckLinksAsync(args.Message);
        IEnumerable<PhishggResponse>? invites = await CheckServerAsync(args.Message);
        
        if (invites is not null)
        {
            foreach (PhishggResponse invite in invites)
            {
                if (invite.IsMatch)
                {
                    await args.Message.RespondAsync($"This message contains at least one link to a suspicious server ({invite.Reason})");
                    break;
                }
            }
        }
        
        if (links is not null && links.IsMatch)
        {
            await args.Message.RespondAsync("This message contains at least one link to a suspicious website");
        }
    }

    private async Task<AntiFishResponse?> CheckLinksAsync(DiscordMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Content))
        {
            return null;
        }

        MatchCollection matches = LinkRegex().Matches(message.Content);

        if (matches.Count == 0)
        {
            return null;
        }

        Logger.LogDebug("Links will be checked");

        var payload = new
        {
            message
        };

        HttpResponseMessage res = await _antiFishClient.PostAsJsonAsync("", payload);

        if (res.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (res.StatusCode != HttpStatusCode.OK)
        {
            Logger.LogError("AntiPhishing failed: {StatusCOde} {Reason}", res.StatusCode.Humanize(), res.ReasonPhrase);
            return null;
        }

        return await res.Content.ReadFromJsonAsync<AntiFishResponse>();
    }

    private async Task<PhishggResponse?> CheckUsernameAsync(string username)
    {
        var payload = new
        {
            username
        };

        HttpResponseMessage res = await _antiFishClient.PostAsJsonAsync("username", payload);
        
        if (res.StatusCode != HttpStatusCode.OK)
        {
            Logger.LogError("AntiPhishing failed: {StatusCOde} {Reason}", res.StatusCode.Humanize(), res.ReasonPhrase);
            return null;
        }
        
        return await res.Content.ReadFromJsonAsync<PhishggResponse>();
    }

    private async Task<IEnumerable<PhishggResponse>?> CheckServerAsync(DiscordMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Content))
        {
            return null;
        }

        MatchCollection matches = InviteRegex().Matches(message.Content);

        if (matches.Count == 0)
        {
            return null;
        }

        List<PhishggResponse> responses = [];

        foreach (Match match in matches)
        {
            DiscordInvite invite = await _discordClient.GetInviteByCodeAsync(match.Groups["code"].Value);
            
            HttpResponseMessage res = await _phishggClient.GetAsync($"server?id={invite.Guild.Id}");
        
            if (res.StatusCode != HttpStatusCode.OK)
            {
                Logger.LogError("PhishGG failed: {StatusCOde} {Reason}", res.StatusCode.Humanize(), res.ReasonPhrase);
                continue;
            }
            Console.WriteLine(await  res.Content.ReadAsStringAsync());
            PhishggResponse? phishggResponse = await res.Content.ReadFromJsonAsync<PhishggResponse>();

            if (phishggResponse is null)
            {
                continue;
            }
            
            responses.Add(phishggResponse);
        }
        
        return responses;
    }
}