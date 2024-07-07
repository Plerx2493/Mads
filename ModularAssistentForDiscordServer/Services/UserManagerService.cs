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

using DSharpPlus.Entities;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;

namespace MADS.Services;

public class UserManagerService
{
    private readonly IDbContextFactory<MadsContext> _dbContextFactory;
    
    public UserManagerService(IDbContextFactory<MadsContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    
    public async ValueTask<UserDbEntity> GetOrCreateUserAsync(DiscordUser discordUser)
    {
        await using MadsContext context = await _dbContextFactory.CreateDbContextAsync();
        
        UserDbEntity? user = await context.Users
            .Include(x => x.Incidents)
            .Include(x => x.Reminders)
            .Include(x => x.VoiceAlerts)
            .FirstOrDefaultAsync(x => x.Id == discordUser.Id);
        
        if (user is null)
        {
            user = new UserDbEntity
            {
                Id = discordUser.Id,
                Username = discordUser.Username
            };
            
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }
        
        return user;
    }
    
    public async ValueTask<UserDbEntity?> GetUserAsync(DiscordUser discordUser)
    {
        await using MadsContext context = await _dbContextFactory.CreateDbContextAsync();
        
        return await context.Users
            .Include(x => x.Incidents)
            .Include(x => x.Reminders)
            .Include(x => x.VoiceAlerts)
            .FirstOrDefaultAsync(x => x.Id == discordUser.Id);
    }
    
    public async ValueTask UpdateUserAsync(UserDbEntity user)
    {
        await using MadsContext context = await _dbContextFactory.CreateDbContextAsync();
        
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }
    
    public async ValueTask DeleteUserAsync(UserDbEntity user)
    {
        await using MadsContext context = await _dbContextFactory.CreateDbContextAsync();
        
        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }
}