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

using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using MADS.CommandsChecks;
using MADS.Services;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace MADS.Commands.Text.Base;

[RequireOwner]
public class Eval
{
    private DiscordCommandService _service;
    
    public Eval(DiscordCommandService service)
    {
        _service = service;
    }
    
    [Command("eval"), Description("Evaluate the result of c# code")]
    public async Task EvalCommand(TextCommandContext ctx, [FromCode] string code)
    {
        string csCode = code;
        
        await ctx.RespondAsync(new DiscordEmbedBuilder()
            .WithColor(new DiscordColor("#FF007F"))
            .WithDescription("💭 Evaluating...")
            .Build());
        
        DiscordMessage? message = await ctx.GetResponseAsync();
        
        try
        {
            TestVariables globalVariables = new(ctx.Client, ctx, _service);
            
            ScriptOptions? scriptOptions = ScriptOptions.Default;
            scriptOptions = scriptOptions.WithImports("System", "System.Collections.Generic", "System.Linq",
                "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities", "DSharpPlus.CommandsNext",
                "DSharpPlus.Interactivity", "DSharpPlus.SlashCommands",
                "MADS", "Humanizer");
            scriptOptions = scriptOptions.WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly =>
                    !assembly.IsDynamic
                    && !string.IsNullOrWhiteSpace(
                        assembly.Location)));
            
            Script<object>? script = CSharpScript.Create(csCode, scriptOptions, typeof(TestVariables));
            script.Compile();
            ScriptState<object>? result = await script.RunAsync(globalVariables);
            if (result?.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
            {
                await message.ModifyAsync(new DiscordEmbedBuilder
                {
                    Title = "✅ Evaluation Result",
                    Description = result.ReturnValue.ToString()!,
                    Color = new DiscordColor("#089FDF")
                }.Build());
            }
            else
            {
                await message.ModifyAsync(new DiscordEmbedBuilder
                {
                    Title = "✅ Evaluation Successful",
                    Description = "No result was returned.",
                    Color = new DiscordColor("#089FDF")
                }.Build());
            }
        }
        catch (Exception ex)
        {
            await message.ModifyAsync(new DiscordEmbedBuilder
            {
                Title = "⚠️ Evaluation Failure",
                Description = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message),
                Color = new DiscordColor("#FF0000")
            }.Build());
        }
    }
}

public class TestVariables
{
    public TestVariables(DiscordClient client, CommandContext ctx, DiscordCommandService mdb)
    {
        Client = client;
        CommandService = mdb;
        Message = ctx is TextCommandContext txtCtx ? txtCtx.Message : null;
        Channel = ctx.Channel;
        Guild = Channel?.Guild;
        User = ctx.User;
        if (Guild is not null && User is not null)
        {
            Member = Guild.GetMemberAsync(User.Id).GetAwaiter().GetResult();
        }
        
        Context = ctx;
    }
    
    public DiscordMessage? Message { get; private set; }
    public DiscordChannel? Channel { get; private set; }
    public DiscordGuild? Guild { get; private set; }
    public DiscordUser? User { get; private set; }
    public DiscordMember? Member { get; private set; }
    public CommandContext Context { get; private set; }
    public DiscordClient Client { get; private set; }
    public DiscordCommandService CommandService { get; private set; }
    public IServiceProvider Services => ModularDiscordBot.Services;
}