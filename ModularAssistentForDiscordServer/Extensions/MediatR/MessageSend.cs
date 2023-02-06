using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;

namespace MADS.Extensions.MediatR;

public class MessageSend
{
    public record MessageSendEvent(DiscordClient Sender, MessageCreateEventArgs Args) : IRequest;
    
    public class SyncHandler : RequestHandler<MessageSendEvent>
    {
        protected override void Handle(MessageSendEvent request)
        {
            Console.WriteLine("Test");
        }
    }
}