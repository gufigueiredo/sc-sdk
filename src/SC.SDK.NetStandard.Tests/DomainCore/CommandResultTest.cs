using System;
using Xunit;
using SC.SDK.NetStandard.DomainCore.Commands;

namespace SC.SDK.NetStandard.Tests.DomainCore
{
    public class CommandResultTest
    {
        [Fact]
        public void Deve_Retornar_Instancia_Chamando_Metodo_Estatico_Ok()
        {
            var instance = CommandResult.Ok();
            Assert.IsType<CommandResult>(instance);
            Assert.True(instance.Success);
        }

        [Fact]
        public void Deve_Retornar_Instancia_Chamando_Metodo_Estatico_Error()
        {
            var instance = CommandResult.Error("ERRO");
            Assert.IsType<CommandResult>(instance);
            Assert.False(instance.Success);
            Assert.Equal("ERRO", instance.Message);
        }

        [Fact]
        public void Deve_Retornar_Not_Found_Chamando_Metodo_Estatico_NotFound()
        {
            var instance = CommandResult.NotFound("RESOURCE", "ID");
            Assert.IsType<CommandResult>(instance);
            Assert.False(instance.Success);
            Assert.Equal(CommandResultError.ResourceNotFound, instance.Reason);
        }
    }
}