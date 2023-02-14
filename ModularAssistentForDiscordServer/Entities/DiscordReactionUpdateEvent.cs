﻿using DSharpPlus.Entities;
using Emzi0767.Utilities;

namespace MADS.Entities;

public class DiscordReactionUpdateEvent
{
    public DiscordMessage Message;
    public AsyncEventArgs EventArgs;
    public DiscordReactionUpdateType Type;
}

public enum DiscordReactionUpdateType
{
    ReactionAdded,
    ReactionRemoved,
    ReactionsCleard,
    ReactionEmojiRemoved
}