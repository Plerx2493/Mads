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

using MADS.Entities;
using Microsoft.EntityFrameworkCore;

namespace MADS.Services;

public class TranslateInformationService
{
    private readonly IDbContextFactory<MadsContext> _dbContextFactory;

    public TranslateInformationService(IDbContextFactory<MadsContext> factory)
    {
        _dbContextFactory = factory;
    }
    
    public async void SetPreferredLanguage(ulong userId, string language)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var user = db.Users.FirstOrDefault(x => x.Id == userId);
        if (user == null) return;
        user.PreferedLanguage = language;
        await db.SaveChangesAsync();
    }
    
    public async Task<string?> GetPreferredLanguage(ulong userId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var user = db.Users.FirstOrDefault(x => x.Id == userId);
        return user?.PreferedLanguage;
    }
    
    internal static string StandardizeLang(string code)
    {
        string[] strArray = code.Split(new char[1]{ '-' }, 2);
        return strArray.Length != 1 ? strArray[0].ToLowerInvariant() + "-" + strArray[1].ToUpperInvariant() : strArray[0].ToLowerInvariant();
    }
}