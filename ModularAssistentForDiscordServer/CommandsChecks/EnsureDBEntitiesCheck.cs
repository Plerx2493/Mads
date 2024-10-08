﻿// Copyright 2023 Plerx2493
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

using System.Diagnostics;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MADS.CommandsChecks;

public class EnsureDBEntitiesCheck : IContextCheck<UnconditionalCheckAttribute>
{
    private IDbContextFactory<MadsContext> _contextFactory;
    private readonly ILogger<EnsureDBEntitiesCheck> _logger;
    
    public EnsureDBEntitiesCheck(IDbContextFactory<MadsContext> dbContextFactory, ILogger<EnsureDBEntitiesCheck> logger)
    {
        _contextFactory = dbContextFactory;
        _logger = logger;
    }
    
    public async ValueTask<string?> ExecuteCheckAsync(UnconditionalCheckAttribute _, CommandContext context)
    {
        Stopwatch sw = Stopwatch.StartNew();

        DiscordUser user = context.User;
        
        await using MadsContext dbContext = await _contextFactory.CreateDbContextAsync();
        
        UserDbEntity userdbEntity = new()
        {
            Id = user.Id,
            Username = user.Username,
            PreferedLanguage = "en-US"
        };
        
        await dbContext.Users.Upsert(userdbEntity)
            .On(x => x.Id)
            .NoUpdate()
            .RunAsync();
        
        if (context.Guild is null)
        {
            return null;
        }
        
        GuildDbEntity guildDbEntity = new(context.Guild.Id);
        
        await dbContext.Guilds.Upsert(guildDbEntity)
            .On(x => x.Id)
            .NoUpdate()
            .RunAsync();
        
        await dbContext.SaveChangesAsync();

        sw.Stop();

        _logger.LogTrace($"EnsureDBEntitiesCheck took {sw.ElapsedMilliseconds} ms");

        return null;
    }
}