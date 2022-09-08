using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace MADS.Modules
{
    internal class DevModule : IMadsModul
    {
        public ModularDiscordBot ModularDiscordClient { get; set; }
        public string ModuleName { get; set; }
        public string ModuleDescription { get; set; }
        public string[] Commands { get; set; }
        public Dictionary<string, string> CommandDescriptions { get; set; }
        public Type CommandClass { get; set; }
        public Type SlashCommandClass { get; set; }

        public DiscordIntents RequiredIntents { get; set; }

        public bool IsHidden { get; init; }

        public DevModule(ModularDiscordBot modularDiscordClient)
        {
            ModularDiscordClient = modularDiscordClient;
            ModuleName = "Dev";
            ModuleDescription = "";
            Commands = new[] { "guild", "channel" };
            CommandDescriptions = new Dictionary<string, string>();
            CommandClass = typeof(DevCommands);
            SlashCommandClass = null;
            RequiredIntents = 0;
            IsHidden = true;
        }
    }

    [RequireOwner, Hidden]
    internal class DevCommands : BaseCommandModule
    {
        public MadsServiceProvider CommandService { get; set; }

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
        public async Task Eval(CommandContext ctx, [RemainingText] string code)
        {
            int codeStart = code.IndexOf("```", StringComparison.Ordinal) + 3;
            codeStart = code.IndexOf('\n', codeStart) + 1;
            var codeEnd = code.LastIndexOf("```", StringComparison.Ordinal);

            if (codeStart == -1 || codeEnd == -1)
                throw new ArgumentException("⚠️ You need to wrap the code into a code block.");

            var csCode = code[codeStart..codeEnd];

            var message = await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#FF007F"))
                .WithDescription("💭 Evaluating...")
                .Build());

            try
            {
                TestVariables globalVariables = new(ctx.Message, ctx.Client, ctx, CommandService.ModularDiscordBot);

                ScriptOptions scriptOptions = ScriptOptions.Default;
                scriptOptions = scriptOptions.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities", "DSharpPlus.CommandsNext", "ModularAssistentForDiscordServer");
                scriptOptions = scriptOptions.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location)));

                Script<object> script = CSharpScript.Create(csCode, scriptOptions, typeof(TestVariables));
                script.Compile();
                ScriptState<object> result = await script.RunAsync(globalVariables);
                if (result?.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
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
        public ModularDiscordBot Mdb { get; set; }

        public TestVariables(DiscordMessage msg, DiscordClient client, CommandContext ctx, ModularDiscordBot mdb)
        {
            Client = client;
            Mdb = mdb;
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