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


    [RequireOwner, GuildIsEnabled("Dev"), Hidden]
    internal class DevCommands : BaseCommandModule
    {
        public ModularDiscordBot ModularDiscordClient { get; set; }

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

            var cs_code = code.Substring(code_start, code_end - code_start);

            message = await context.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#FF007F"))
                .WithDescription("💭 Evaluating...")
                .Build()).ConfigureAwait(false);

            try
            {
                var global_variabls = new TestVariables(context.Message, context.Client, context);

                var scriptoptions = ScriptOptions.Default;
                scriptoptions = scriptoptions.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.CommandsNext");
                scriptoptions = scriptoptions.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location)));

                var script = CSharpScript.Create(cs_code, scriptoptions, typeof(TestVariables));
                script.Compile();
                var result = await script.RunAsync(global_variabls).ConfigureAwait(false);

                if (result != null && result.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                    await message.ModifyAsync(new DiscordEmbedBuilder 
                    { 
                        Title = "✅ Evaluation Result",
                        Description = result.ReturnValue.ToString(),
                        Color = new DiscordColor("#089FDF") 
                    }.Build()).ConfigureAwait(false);
                else
                    await message.ModifyAsync(new DiscordEmbedBuilder 
                    { 
                        Title = "✅ Evaluation Successful",
                        Description = "No result was returned.",
                        Color = new DiscordColor("#089FDF") 
                    }.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await message.ModifyAsync(embed: new DiscordEmbedBuilder 
                { 
                    Title = "⚠️ Evaluation Failure",
                    Description = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message),
                    Color = new DiscordColor("#FF0000") 
                }.Build()).ConfigureAwait(false);
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

        public DiscordClient Client;

        public TestVariables(DiscordMessage msg, DiscordClient client, CommandContext ctx)
        {
            this.Client = client;

            this.Message = msg;
            this.Channel = msg.Channel;
            this.Guild = this.Channel.Guild;
            this.User = this.Message.Author;
            if (this.Guild != null)
                this.Member = this.Guild.GetMemberAsync(this.User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
            this.Context = ctx;
        }

    }
}
