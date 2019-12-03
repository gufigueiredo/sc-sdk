using System;

namespace SC.SDK.NetStandard.DomainCore
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        void Rollback();
    }
}