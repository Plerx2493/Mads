using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace MADS.Commands.Text;

[RequireOwner, Hidden]
internal class DevCommands : BaseCommandModule
{
    public MadsServiceProvider CommandService { get; set; }

    [Command("guild")]
    public async Task GetGuild(CommandContext ctx, ulong id)
    {
        var tmp = await ctx.Client.GetGuildAsync(id);
        await ctx.RespondAsync($"Guild: {tmp.Name}");
    }

    [Command("channel")]
    public async Task GetChannel(CommandContext ctx, ulong id)
    {
        var tmp = await ctx.Client.GetChannelAsync(id);
        await ctx.RespondAsync($"Channel: {tmp.Name}");
    }

    [Command("eval"), Description("Evaluate the result of c# code")]
    public async Task Eval(CommandContext ctx, [RemainingText] string code)
    {
        var codeStart = code.IndexOf("```", StringComparison.Ordinal) + 3;
        codeStart = code.IndexOf('\n', codeStart) + 1;
        var codeEnd = code.LastIndexOf("```", StringComparison.Ordinal);

        if (codeStart == -1 || codeEnd == -1)
        {
            throw new ArgumentException("⚠️ You need to wrap the code into a code block.");
        }

        var csCode = code[codeStart..codeEnd];

        var message = await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                                                    .WithColor(new DiscordColor("#FF007F"))
                                                    .WithDescription("💭 Evaluating...")
                                                    .Build());

        try
        {
            TestVariables globalVariables = new(ctx.Message, ctx.Client, ctx, CommandService.ModularDiscordBot);

            var scriptOptions = ScriptOptions.Default;
            scriptOptions = scriptOptions.WithImports("System", "System.Collections.Generic", "System.Linq",
                "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities", "DSharpPlus.CommandsNext",
                "MADS");
            scriptOptions = scriptOptions.WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                                                                  .Where(assembly =>
                                                                      !assembly.IsDynamic
                                                                      && !string.IsNullOrWhiteSpace(
                                                                          assembly.Location)));

            var script = CSharpScript.Create(csCode, scriptOptions, typeof(TestVariables));
            script.Compile();
            var result = await script.RunAsync(globalVariables);
            if (result?.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
            {
                await message.ModifyAsync(new DiscordEmbedBuilder
                {
                    Title = "✅ Evaluation Result",
                    Description = result.ReturnValue.ToString(),
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
    public TestVariables(DiscordMessage msg, DiscordClient client, CommandContext ctx, ModularDiscordBot mdb)
    {
        Client = client;
        Mdb = mdb;
        Message = msg;
        Channel = msg.Channel;
        Guild = Channel.Guild;
        User = Message.Author;
        if (Guild != null)
            Member = Guild.GetMemberAsync(User.Id).GetAwaiter().GetResult();
        Context = ctx;
    }

    public DiscordMessage    Message { get; set; }
    public DiscordChannel    Channel { get; set; }
    public DiscordGuild      Guild   { get; set; }
    public DiscordUser       User    { get; set; }
    public DiscordMember     Member  { get; set; }
    public CommandContext    Context { get; set; }
    public DiscordClient     Client  { get; set; }
    public ModularDiscordBot Mdb     { get; set; }
}