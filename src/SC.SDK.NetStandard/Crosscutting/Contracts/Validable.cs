using Flunt.Notifications;
using Flunt.Validations;
using System;
using System.Linq.Expressions;

namespace SC.SDK.NetStandard.Crosscutting.Contracts
{
    public class Validable : Notifiable
    {
        private object _value;
        private string _property;

        public Validable CreateInstance(object value, string property)
        {
            _value = value;
            _property = property;
            return this;
        }
        public Validable AddValidationFor<T>(Expression<Func<T>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression), "Expressão para validação deve ser informada");

            _property = ((MemberExpression)expression.Body).Member.Name;
            _value = expression.Compile()();
            return this;
        }

        //public Validable MergeNotifications()
        //{

        //}

        public Validable IsNotNull()
        {
            AddNotifications(new Contract()
              .Requires()
              .IsNotNull(_value, _property, $"O parâmetro {_property} não pode ser null")
            );

            return this;
        }

        public Validable IsNotNullOrWhiteSpace()
        {
            if (_value != null && !(_value is string))
            {
                AddNotification(_property, "IsNotNullOrWhiteSpace: O valor informado não é do tipo string");
            }
            else
            {
                AddNotifications(new Contract()
                  .Requires()
                  .IsNotNullOrWhiteSpace(_value as string, _property, $"O parâmetro {_property} não pode ser null ou vazio")
                );
            }

            return this;
        }

        public Validable HasLength(int length)
        {
            if (_value != null && !(_value is string))
            {
                AddNotification(_property, "HasLength: O valor informado não é do tipo string");
            }
            else
            {
                AddNotifications(new Contract()
                  .Requires()
                  .HasLen(_value as string, length, _property, $"O parâmetro {_property} não pode ser null ou vazio")
                );
            }

            return this;
        }

        public Validable HasMaxLength(int maxLength)
        {
            if (_value != null && !(_value is string))
            {
                AddNotification(_property, "HasMaxLength: O valor informado não é do tipo string");
            }
            else
            {
                AddNotifications(new Contract()
                  .Requires()
                  .HasMaxLen(_value as string, maxLength, _property, $"O parâmetro {_property} não pode ser null ou vazio")
                );
            }

            return this;
        }

        public Validable HasMinLength(int minLength)
        {
            if (_value != null && !(_value is string))
            {
                AddNotification(_property, "HasMinLength: O valor informado não é do tipo string");
            }
            else
            {
                AddNotifications(new Contract()
                  .Requires()
                  .HasMinLen(_value as string, minLength, _property, $"O parâmetro {_property} não pode ser null ou vazio")
                );
            }

            return this;
        }

        public Validable IsGreaterThan(int value)
        {
            if (_value != null && !(_value is int))
            {
                AddNotification(_property, "IsGreaterThan: O valor informado não é do tipo inteiro");
            }
            else
            {
                AddNotifications(new Contract()
                  .Requires()
                  .IsGreaterThan((int)_value, value, _property, $"O parâmetro {_property} não pode ser null ou vazio")
                );
            }

            return this;
        }

        public Validable IsGreaterThan(decimal value)
        {
            if (_value != null && !(_value is decimal))
            {
                AddNotification(_property, "IsGreaterThan: O valor informado não é do tipo decimal");
            }
            else
            {
                AddNotifications(new Contract()
                  .Requires()
                  .IsGreaterThan((decimal)_value, value, _property, $"O parâmetro {_property} não pode ser null ou vazio")
                );
            }

            return this;
        }

        public Validable IsGreaterOrEqualsThan(int value)
        {
            if (_value != null && !(_value is int))
            {
                AddNotification(_property, "IsGreaterOrEqualsThan: O valor informado não é do tipo inteiro");
            }
            else
            {
                AddNotifications(new Contract()
                  .Requires()
                  .IsGreaterOrEqualsThan((int)_value, value, _property, $"O parâmetro {_property} não pode ser null ou vazio")
                );
            }

            return this;
        }

        public Validable IsGreaterOrEqualsThan(decimal value)
        {
            if (_value != null && !(_value is decimal))
            {
                AddNotification(_property, "IsGreaterOrEqualsThan: O valor informado não é do tipo decimal");
            }
            else
            {
                AddNotifications(new Contract()
                  .Requires()
                  .IsGreaterOrEqualsThan((decimal)_value, value, _property, $"O parâmetro {_property} não pode ser null ou vazio")
                );
            }

            return this;
        }

        public Validable IsLowerThan(int value)
        {
            if (_value != null && !(_value is int))
            {
                AddNotification(_property, "IsLowerThan: O valor informado não é do tipo inteiro");
            }
            else
            {
                AddNotifications(new Contract()
                  .Requires()
                  .IsLowerThan((int)_value, value, _property, $"O parâmetro {_property} não pode ser null ou vazio")
                );
            }

            return this;
        }

        public Validable IsLowerThan(decimal value)
        {
            if (_value != null && !(_value is decimal))
            {
                AddNotification(_property, "IsLowerThan: O valor informado não é do tipo decimal");
            }
            else
            {
                AddNotifications(new Contract()
                  .Requires()
                  .IsLowerThan((decimal)_value, value, _property, $"O parâmetro {_property} não pode ser null ou vazio")
                );
            }

            return this;
        }

        public Validable IsLowerOrEqualsThan(int value)
        {
            if (_value != null && !(_value is int))
            {
                AddNotification(_property, "IsLowerOrEqualsThan: O valor informado não é do tipo inteiro");
            }
            else
            {
                AddNotifications(new Contract()
                  .Requires()
                  .IsLowerOrEqualsThan((int)_value, value, _property, $"O parâmetro {_property} não pode ser null ou vazio")
                );
            }

            return this;
        }

        public Validable IsLowerOrEqualsThan(decimal value)
        {
            if (_value != null && !(_value is decimal))
            {
                AddNotification(_property, "IsLowerOrEqualsThan: O valor informado não é do tipo decimal");
            }
            else
            {
                AddNotifications(new Contract()
                  .Requires()
                  .IsLowerOrEqualsThan((decimal)_value, value, _property, $"O parâmetro {_property} não pode ser null ou vazio")
                );
            }

            return this;
        }

        public Validable IsValidDateTime()
        {
            if (_value == null)
            {
                AddNotification(_property, "IsNotDefaultDateTimeValue: O valor informado não pode ser null");
            }
            else if (_value != null && !(_value is DateTime))
            {
                AddNotification(_property, "IsNotDefaultDateTimeValue: O valor informado não é do tipo DateTime");
            }
            else
            {
                if ((DateTime)_value == DateTime.MinValue)
                {
                    AddNotification(_property, $"IsNotDefaultDateTimeValue: O parâmetro {_property} não é uma data válida");
                }
            }

            return this;
        }
    }
}
