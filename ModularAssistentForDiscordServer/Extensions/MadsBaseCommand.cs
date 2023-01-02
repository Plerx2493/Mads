using System.Diagnostics;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using MADS.Commands;

namespace MADS.Extensions;

public class MadsBaseCommand : BaseCommandModule
{
    private readonly Stopwatch           _executionTimer = new();
    public           ModularDiscordBot CommandService { get; set; }

    public override Task BeforeExecutionAsync(CommandContext ctx)
    {
        _executionTimer.Restart();

        return Task.FromResult(true);
    }

    public override Task AfterExecutionAsync(CommandContext ctx)
    {
        _executionTimer.Stop();

        CommandService.Logging.LogCommandExecutionAsync(ctx, _executionTimer.Elapsed);


        return Task.FromResult(true);
    }
}