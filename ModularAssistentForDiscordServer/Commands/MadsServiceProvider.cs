namespace MADS.Commands;

public class MadsServiceProvider
{
    public ModularDiscordBot ModularDiscordBot;

    public MadsServiceProvider(ModularDiscordBot modularDiscordBot)
    {
        ModularDiscordBot = modularDiscordBot;
    }
}