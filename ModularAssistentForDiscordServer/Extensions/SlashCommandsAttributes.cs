using DSharpPlus.SlashCommands;

namespace MADS.Extensions;

public sealed class SlashRequireOwnerAttribute : SlashCheckBaseAttribute
{
    public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        var app = ctx.Client.CurrentApplication;
        var me = ctx.Client.CurrentUser;

        return app != null
            ? Task.FromResult(app.Owners.Any(x => x.Id == ctx.User.Id))
            : Task.FromResult(ctx.User.Id == me.Id);
    }
}