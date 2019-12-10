namespace SC.SDK.NetStandard.DomainCore
{
    public interface IRepository<T> where T : IAggregateRoot
    {
        void Add(T entity);
        void Update(T entity);
        void Remove(string objectKey);
    }
}