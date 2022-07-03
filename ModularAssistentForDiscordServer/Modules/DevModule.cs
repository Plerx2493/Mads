using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using Microsoft.CodeAnalysis;
using MADS;
using MADS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace MADS.Modules
{
    internal class DevModule : IMadsModul
    {
        public ModularDiscordBot ModularDiscordClient { get; set; }
        public string ModulName { get; set; }
        public string ModulDescription { get; set; }
        public string[] Commands { get; set; }
        public Dictionary<string, string> CommandDescriptions { get; set; }
        public Type CommandClass { get; set; }
        public Type SlashCommandClass { get; set; }

        public DiscordIntents RequiredIntents { get; set; }

        public bool IsHidden { get; init; }

        public DevModule(ModularDiscordBot modularDiscordClient)
        {
            ModularDiscordClient = modularDiscordClient;
            ModulName = "Dev";
            ModulDescription = "";
            Commands = new string[] { "guild", "channel" };
            CommandDescriptions = new();
            CommandClass = typeof(DevCommands);
            SlashCommandClass = null;
            RequiredIntents = 0;
            IsHidden = true;
            
        }
    }


    [RequireOwner, Hidden]
    internal class DevCommands : BaseCommandModule
    {
        public MadsServiceProvider CommandService{ get; set; }

        [Command("guild")]
        public async Task GetGuild(CommandContext ctx, ulong id)
        {
            var tmp = await ctx.Client.GetChannelAsync(id);
            await ctx.RespondAsync($"Guild: {tmp.Name}");
        }

        [Command("channel")]
        public async Task GetChannel(CommandContext ctx, ulong id)
        {
            var tmp = await ctx.Client.GetChannelAsync(id);
            await ctx.RespondAsync($"Channel: {tmp.Name}");
        }

        [Command("user")]
        public async Task GetUser(CommandContext ctx, ulong id)
        {
            var tmp = await ctx.Client.GetUserAsync(id);
            await ctx.RespondAsync($"User: {tmp.Username}");
        }

        [Command("eval"), Description("Evaluate the result of c# code")]
        public async Task Eval(CommandContext context, [RemainingText] string code)
        {


            var message = context.Message;

            var code_start = code.IndexOf("```") + 3;
            code_start = code.IndexOf('\n', code_start) + 1;
            var code_end = code.LastIndexOf("```");

            if (code_start == -1 || code_end == -1)
                throw new ArgumentException("⚠️ You need to wrap the code into a code block.");

            var cs_code = code[code_start..code_end];

            message = await context.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#FF007F"))
                .WithDescription("💭 Evaluating...")
                .Build());

            try
            {
                TestVariables global_variabls = new(context.Message, context.Client, context, CommandService.modularDiscordBot);
                
                ScriptOptions scriptoptions = ScriptOptions.Default;
                scriptoptions = scriptoptions.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities", "DSharpPlus.CommandsNext", "ModularAssistentForDiscordServer");
                scriptoptions = scriptoptions.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location)));

                Script<object> script = CSharpScript.Create(cs_code, scriptoptions, typeof(TestVariables));
                script.Compile();
                ScriptState<object> result = await script.RunAsync(global_variabls);
                if (result != null && result.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                    await message.ModifyAsync(new DiscordEmbedBuilder 
                    { 
                        Title = "✅ Evaluation Result",
                        Description = result.ReturnValue.ToString(),
                        Color = new DiscordColor("#089FDF") 
                    }.Build());
                else
                    await message.ModifyAsync(new DiscordEmbedBuilder 
                    { 
                        Title = "✅ Evaluation Successful",
                        Description = "No result was returned.",
                        Color = new DiscordColor("#089FDF") 
                    }.Build());
                
            }
            catch (Exception ex)
            {
                await message.ModifyAsync(embed: new DiscordEmbedBuilder 
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
        public DiscordMessage Message { get; set; }
        public DiscordChannel Channel { get; set; }
        public DiscordGuild Guild { get; set; }
        public DiscordUser User { get; set; }
        public DiscordMember Member { get; set; }
        public CommandContext Context { get; set; }

        public DiscordClient Client { get; set; }

        public ModularDiscordBot MDB { get; set; }

        public TestVariables(DiscordMessage msg, DiscordClient client, CommandContext ctx, ModularDiscordBot mdb)
        {
            Client = client;
            MDB = mdb;
            Message = msg;
            Channel = msg.Channel;
            Guild = Channel.Guild;
            User = Message.Author;
            if (Guild != null)
                Member = Guild.GetMemberAsync(User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
            Context = ctx;
        }

    }
}
