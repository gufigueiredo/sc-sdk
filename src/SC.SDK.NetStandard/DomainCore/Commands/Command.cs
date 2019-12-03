using MediatR;
using Flunt.Notifications;

namespace SC.SDK.NetStandard.DomainCore.Commands
{
    public abstract class Command : Notifiable, IRequest<CommandResult>
    {
        public abstract void Validate();
    }
}