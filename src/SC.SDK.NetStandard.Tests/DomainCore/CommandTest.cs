using System;
using Flunt.Validations;
using Xunit;

namespace SC.SDK.NetStandard.Tests
{
    public class CommandTest
    {
        [Fact]
        public void Deve_Implementar_Override_Validate()
        {
            var instance = new CommandDerivedClass();
            Assert.Throws<NotImplementedException>(() => instance.Validate());

        }

        [Fact]
        public void Deve_Iniciar_Instancia_Com_Estado_Valido()
        {
            var instance = new CommandDerivedClass();
            Assert.True(instance.Valid);
        }

        [Fact]
        public void Deve_Adicionar_Notifications_E_Retornar_Estado_Invalido()
        {
            var instance = new CommandDerivedClass();
            instance.AddNotification("PROPRIEDADE", "MENSAGEM");
            Assert.True(instance.Invalid);
            Assert.Single(instance.Notifications);
        }
    }

    internal class CommandDerivedClass : SC.SDK.NetStandard.DomainCore.Commands.Command
    {
        public override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
