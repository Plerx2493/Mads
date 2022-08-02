using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MADS.Entities
{
    public enum ActionDiscordButtonEnum : byte
    {
        BanUser,
        KickUser,
        GetIDUser,
        GetIDChannel,
        GetIDGuild,
        MoveVoiceChannel
    }
}
