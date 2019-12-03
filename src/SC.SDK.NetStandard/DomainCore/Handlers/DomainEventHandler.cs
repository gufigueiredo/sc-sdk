using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SC.SDK.NetStandard.DomainCore.Events;

namespace SC.SDK.NetStandard.DomainCore.Handlers
{
    public abstract class DomainEventHandler<TEvent> : INotificationHandler<TEvent>
        where TEvent : Event
    {
        public abstract Task Handle(TEvent notification, CancellationToken cancellationToken);
    }
}
