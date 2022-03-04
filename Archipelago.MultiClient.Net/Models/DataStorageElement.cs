using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using System;

namespace Archipelago.MultiClient.Net.Models
{
    public class DataStorageElement
    {
        public event DataStorageHelper.DataStorageUpdatedHandler OnValueChanged
        {
            add => Context.AddHandler(Context.Key, value);
            remove => Context.RemoveHandler(Context.Key, value);
        }

        internal DataStorageElementContext Context;
        internal Operation Operation;
        internal object Value;

        private object cachedValue;

        internal DataStorageElement() { }

        internal DataStorageElement(DataStorageElementContext context)
        {
            Context = context;
        }

        public static DataStorageElement operator ++(DataStorageElement a)
        {
            return new DataStorageElement { Operation = Operation.Add, Value = 1, Context = a.Context };
        }

        public static DataStorageElement operator --(DataStorageElement a)
        {
            return new DataStorageElement { Operation = Operation.Add, Value = -1, Context = a.Context };
        }

        public static DataStorageElement operator +(DataStorageElement a, int b)
        {
            return new DataStorageElement { Operation = Operation.Add, Value = b, Context = a.Context };
        }

        public static DataStorageElement operator +(DataStorageElement a, long b)
        {
            return new DataStorageElement { Operation = Operation.Add, Value = b, Context = a.Context };
        }

        public static DataStorageElement operator +(DataStorageElement a, decimal b)
        {
            return new DataStorageElement { Operation = Operation.Add, Value = b, Context = a.Context };
        }

        public static DataStorageElement operator +(DataStorageElement a, double b)
        {
            return new DataStorageElement { Operation = Operation.Add, Value = b, Context = a.Context };
        }

        public static DataStorageElement operator +(DataStorageElement a, float b)
        {
            return new DataStorageElement { Operation = Operation.Add, Value = b, Context = a.Context };
        }

        public static DataStorageElement operator +(DataStorageElement a, string b)
        {
            return new DataStorageElement { Operation = Operation.Add, Value = b, Context = a.Context };
        }

        public static DataStorageElement operator -(DataStorageElement a, int b)
        {
            return new DataStorageElement { Operation = Operation.Add, Value = -b, Context = a.Context };
        }

        public static DataStorageElement operator -(DataStorageElement a, long b)
        {
            return new DataStorageElement { Operation = Operation.Add, Value = -b, Context = a.Context };
        }

        public static DataStorageElement operator -(DataStorageElement a, decimal b)
        {
            return new DataStorageElement { Operation = Operation.Add, Value = -b, Context = a.Context };
        }

        public static DataStorageElement operator -(DataStorageElement a, double b)
        {
            return new DataStorageElement { Operation = Operation.Add, Value = -b, Context = a.Context };
        }

        public static DataStorageElement operator -(DataStorageElement a, float b)
        {
            return new DataStorageElement { Operation = Operation.Add, Value = -b, Context = a.Context };
        }

        public static DataStorageElement operator *(DataStorageElement a, int b)
        {
            return new DataStorageElement { Operation = Operation.Mul, Value = b, Context = a.Context };
        }

        public static DataStorageElement operator *(DataStorageElement a, long b)
        {
            return new DataStorageElement { Operation = Operation.Mul, Value = b, Context = a.Context };
        }

        public static DataStorageElement operator *(DataStorageElement a, decimal b)
        {
            return new DataStorageElement { Operation = Operation.Mul, Value = b, Context = a.Context };
        }

        public static DataStorageElement operator *(DataStorageElement a, double b)
        {
            return new DataStorageElement { Operation = Operation.Mul, Value = b, Context = a.Context };
        }

        public static DataStorageElement operator *(DataStorageElement a, float b)
        {
            return new DataStorageElement { Operation = Operation.Mul, Value = b, Context = a.Context };
        }

        public static DataStorageElement operator /(DataStorageElement a, int b)
        {
            return new DataStorageElement { Operation = Operation.Mul, Value = 1m / b, Context = a.Context };
        }

        public static DataStorageElement operator /(DataStorageElement a, long b)
        {
            return new DataStorageElement { Operation = Operation.Mul, Value = 1m / b, Context = a.Context };
        }

        public static DataStorageElement operator /(DataStorageElement a, decimal b)
        {
            return new DataStorageElement { Operation = Operation.Mul, Value = 1m / b, Context = a.Context };
        }

        public static DataStorageElement operator /(DataStorageElement a, double b)
        {
            return new DataStorageElement { Operation = Operation.Mul, Value = 1d / b, Context = a.Context };
        }

        public static DataStorageElement operator /(DataStorageElement a, float b)
        {
            return new DataStorageElement { Operation = Operation.Mul, Value = 1d / b, Context = a.Context };
        }

        public static DataStorageElement operator >>(DataStorageElement a, int b)
        {
            return new DataStorageElement { Operation = Operation.Min, Value = b, Context = a.Context };
        }

        public static DataStorageElement operator <<(DataStorageElement a, int b)
        {
            return new DataStorageElement { Operation = Operation.Max, Value = b, Context = a.Context };
        }

        public static implicit operator DataStorageElement(int i)
        {
            return new DataStorageElement { Operation = Operation.Replace, Value = i };
        }

        public static implicit operator DataStorageElement(long l)
        {
            return new DataStorageElement { Operation = Operation.Replace, Value = l };
        }

        public static implicit operator DataStorageElement(decimal l)
        {
            return new DataStorageElement { Operation = Operation.Replace, Value = l };
        }

        public static implicit operator DataStorageElement(double l)
        {
            return new DataStorageElement { Operation = Operation.Replace, Value = l };
        }

        public static implicit operator DataStorageElement(float l)
        {
            return new DataStorageElement { Operation = Operation.Replace, Value = l };
        }

        public static implicit operator DataStorageElement(string s)
        {
            return new DataStorageElement { Operation = Operation.Replace, Value = s };
        }

        public static implicit operator int(DataStorageElement e)
        {
            return RetrieveAndReturnValue<int>(e);
        }

        public static implicit operator long(DataStorageElement e)
        {
            return RetrieveAndReturnValue<long>(e);
        }

        public static implicit operator decimal(DataStorageElement e)
        {
            return RetrieveAndReturnValue<decimal>(e);
        }

        public static implicit operator double(DataStorageElement e)
        {
            return RetrieveAndReturnValue<double>(e);
        }

        public static implicit operator float(DataStorageElement e)
        {
            return RetrieveAndReturnValue<float>(e);
        }

        public static implicit operator string(DataStorageElement e)
        {
            return RetrieveAndReturnValue(e);
        }

        private static string RetrieveAndReturnValue(DataStorageElement e)
        {
            if (e.cachedValue != null)
            {
                return (string)e.cachedValue;
            }

            string value;
            string serverValue = Convert.ToString(e.Context.GetData(e.Context.Key));

            switch (e.Operation)
            {
                case Operation.Add:
                    value = serverValue + Convert.ToString(e.Value);
                    break;

                case Operation.Default:
                    value = serverValue;
                    break;

                default:
                    throw new InvalidOperationException($"Cannot perform operation {e.Operation} on string value");
            }

            e.cachedValue = value;

            return Convert.ToString(e.cachedValue);
        }

        private static T RetrieveAndReturnValue<T>(DataStorageElement e) where T : struct
        {
            if (e.cachedValue != null)
            {
                return (T)e.cachedValue;
            }

            decimal value;
            decimal serverValue = Convert.ToDecimal(e.Context.GetData(e.Context.Key));

            switch (e.Operation)
            {
                case Operation.Add:
                    value = serverValue + Convert.ToDecimal(e.Value);
                    break;

                case Operation.Mul:
                    value = serverValue * Convert.ToDecimal(e.Value);
                    break;

                case Operation.Max:
                    value = Math.Max(serverValue, Convert.ToDecimal(e.Value));
                    break;

                case Operation.Min:
                    value = Math.Min(serverValue, Convert.ToDecimal(e.Value));
                    break;

                default:
                    value = serverValue;
                    break;
            }

            e.cachedValue = value;

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}