using Flunt.Notifications;

namespace SC.SDK.NetStandard.DomainCore
{
    public abstract class ValueObject : Notifiable
    {
        public ValueObject GetCopy()
        {
            return this.MemberwiseClone() as ValueObject;
        }
    }
}