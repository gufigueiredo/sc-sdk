using Flunt.Notifications;
using SC.SDK.NetStandard.Crosscutting.Contracts;

namespace SC.SDK.NetStandard.DomainCore
{
    public abstract class ValueObject : Validable
    {
        public ValueObject GetCopy()
        {
            return this.MemberwiseClone() as ValueObject;
        }
    }
}