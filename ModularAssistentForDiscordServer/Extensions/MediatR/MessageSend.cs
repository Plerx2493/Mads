using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;

namespace MADS.Extensions.MediatR;

public class MessageSend
{
    public record MessageSendEvent(DiscordClient Sender, MessageCreateEventArgs Args) : IRequest;

    public class SyncHandler : IRequestHandler<MessageSendEvent>
    {
        public Task Handle(MessageSendEvent request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Test");
            return Task.CompletedTask;
        }
    }
}