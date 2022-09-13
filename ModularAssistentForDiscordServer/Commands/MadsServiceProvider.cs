namespace MADS.Commands;

internal class MadsServiceProvider
{
    public ModularDiscordBot ModularDiscordBot;
    public MadsServiceProvider(ModularDiscordBot modularDiscordBot)
    {
        ModularDiscordBot = modularDiscordBot;
    }
}