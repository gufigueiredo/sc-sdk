using System;
using System.Data;

namespace SC.SDK.NetStandard.Infrastructure.Repository
{
    public abstract class RepositoryBase
    {
        protected readonly IDbConnection _connection;
        protected readonly IDbTransaction _transaction;

        protected RepositoryBase(IDbConnection connection)
        {
            _connection = connection;
        }

        protected RepositoryBase(IDbTransaction transaction)
        {
            _transaction = transaction;
        }
    }
}
