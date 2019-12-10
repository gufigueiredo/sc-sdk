using System;
using System.Collections.Generic;
using MediatR;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SC.SDK.NetStandard.Crosscutting.Contracts;

namespace SC.SDK.NetStandard.DomainCore
{
    public abstract class Entity : Validable
    {
        private Guid _id;
        public virtual Guid Id
        {
            get => _id;
            protected set => _id = value;
        }

        protected Entity() => Id = Guid.NewGuid();

        private List<INotification> _domainEvents;

        [JsonIgnore]
        public IReadOnlyCollection<INotification> DomainEvents => _domainEvents?.AsReadOnly();

        protected void AddDomainEvent(INotification eventItem)
        {
            _domainEvents = _domainEvents ?? new List<INotification>();
            _domainEvents.Add(eventItem);
        }

        protected void RemoveDomainEvent(INotification eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }

        protected void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }

        public async Task RaiseEvents(IMediator mediator)
        {
            foreach (var e in DomainEvents)
            {
                await mediator.Publish(e);
            }
        }
    }
}