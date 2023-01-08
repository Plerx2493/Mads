using System.Diagnostics;
using DSharpPlus.CommandsNext;

namespace MADS.Extensions;

public class MadsBaseCommand : BaseCommandModule
{
    private readonly Stopwatch         _executionTimer = new();
    public           ModularDiscordBot CommandService { get; set; }

    public override Task BeforeExecutionAsync(CommandContext ctx)
    {
        _executionTimer.Restart();

        return Task.FromResult(true);
    }

    public override Task AfterExecutionAsync(CommandContext ctx)
    {
        _executionTimer.Stop();

        var _ = CommandService.Logging.LogCommandExecutionAsync(ctx, _executionTimer.Elapsed);


        return Task.FromResult(true);
    }
    
    public async Task IntendedWait(int milliseconds)
    {
        _executionTimer.Stop();

        await Task.Delay(milliseconds);
        
        _executionTimer.Start();
    }
}