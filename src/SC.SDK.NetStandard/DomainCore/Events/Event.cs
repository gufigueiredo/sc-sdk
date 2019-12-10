using System;
using MediatR;

namespace SC.SDK.NetStandard.DomainCore.Events
{
    public abstract class Event : INotification
    {
        public DateTime TimeStamp { get; private set; }
        public Guid Id { get; private set; }

        protected Event()
        {
            TimeStamp = DateTime.Now;
            Id = Guid.NewGuid();
        }
        
    }
}