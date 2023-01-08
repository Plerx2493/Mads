using System.Diagnostics;
using DSharpPlus.SlashCommands;

namespace MADS.Extensions;

[SlashModuleLifespan(SlashModuleLifespan.Transient)]
public class MadsBaseApplicationCommand : ApplicationCommandModule
{
    private readonly Stopwatch         _executionTimer = new();
    public           ModularDiscordBot CommandService { get; set; }

    public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        _executionTimer.Restart();

        return Task.FromResult(true);
    }

    public override Task AfterSlashExecutionAsync(InteractionContext ctx)
    {
        _executionTimer.Stop();

        var _ = CommandService.Logging.LogCommandExecutionAsync(ctx, _executionTimer.Elapsed);


        return Task.FromResult(true);
    }

    public override Task<bool> BeforeContextMenuExecutionAsync(ContextMenuContext ctx)
    {
        _executionTimer.Restart();

        return Task.FromResult(true);
    }

    public override Task AfterContextMenuExecutionAsync(ContextMenuContext ctx)
    {
        _executionTimer.Stop();

        var _ = CommandService.Logging.LogCommandExecutionAsync(ctx, _executionTimer.Elapsed);

        return Task.FromResult(true);
    }
}