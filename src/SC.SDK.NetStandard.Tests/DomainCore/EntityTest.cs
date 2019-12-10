using System;
using MediatR;
using Moq;
using SC.SDK.NetStandard.DomainCore;
using Xunit;

namespace SC.SDK.NetStandard.Tests.DomainCore
{
    public class EntityTest
    {
        [Fact]
        public void Deve_Adicionar_Domain_Event()
        {
            var instance = new EntityDerivedClass();
            var domainEvent = new Mock<INotification>();
            instance.SimulateEvent(domainEvent.Object);
            Assert.Single(instance.DomainEvents);
        }

        [Fact]
        public void Deve_Gerar_UID_Ao_Instanciar()
        {
            var instance = new EntityDerivedClass();
            Assert.NotEqual(Guid.Empty, instance.Id);
        }
    }

    internal class EntityDerivedClass : Entity 
    {
        public void SimulateEvent(INotification notification) => AddDomainEvent(notification); 
    }

}
