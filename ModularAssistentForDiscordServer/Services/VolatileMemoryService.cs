using DSharpPlus.Entities;

namespace MADS.Services;

public class VolatileMemoryService
{
    private List<ulong> _voiceTrollUser;

    public VolatileMemoryService(List<ulong> voiceTrollUsers)
    {
        _voiceTrollUser = voiceTrollUsers;
    }

    public VolatileMemoryService()
    {
        _voiceTrollUser = new List<ulong>();
    }

    public void AddVoiceTrollUser(DiscordUser user) 
    {
        if (!_voiceTrollUser.Contains(user.Id)) _voiceTrollUser.Add(user.Id);
    }

    public void DeleteVoiceTrollUser(DiscordUser user)
    {
        _voiceTrollUser.RemoveAll(x => user.Id == x);
    }

    public bool IsVoiceTrollUser(DiscordUser user)
    {
        return _voiceTrollUser.Contains(user.Id);
    }
}