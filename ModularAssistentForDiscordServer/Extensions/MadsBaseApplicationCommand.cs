using System.Diagnostics;
using DSharpPlus.SlashCommands;
using MADS.Commands;

namespace MADS.Extensions;

[SlashModuleLifespan(SlashModuleLifespan.Transient)]
public class MadsBaseApplicationCommand : ApplicationCommandModule
{
    private readonly Stopwatch           _executionTimer = new();
    public           MadsServiceProvider CommandService { get; set; }

    public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        _executionTimer.Restart();

        return Task.FromResult(true);
    }

    public override Task AfterSlashExecutionAsync(InteractionContext ctx)
    {
        _executionTimer.Stop();

        //Do logging stuff here


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

        //Do logging stuff here

        return Task.FromResult(true);
    }
}