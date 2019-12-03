using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flunt.Notifications;
using MediatR;
using SC.SDK.NetStandard.DomainCore.Commands;

namespace SC.SDK.NetStandard.DomainCore.Handlers
{
    public abstract class CommandHandler<TCommand> : Notifiable, IRequestHandler<TCommand, CommandResult>
        where TCommand : IRequest<CommandResult>
    {
        public abstract Task<CommandResult> Handle(TCommand request, CancellationToken cancellationToken);
    }
}