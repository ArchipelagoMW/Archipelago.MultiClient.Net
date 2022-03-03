using Archipelago.MultiClient.Net.Helpers;
using System;

namespace Archipelago.MultiClient.Net.Models
{
    class DataStorageElement
    {
        public event DataStorageHelper.DataStorageUpdatedHandler OnValueChanged
        {
            add => Context.AddHandler(Context.Key, value);
            remove => Context.RemoveHandler(Context.Key, value);
        }

        public DataStorageElementContext Context;

        public string Method { get; set; }
        public object Value { get; set; }

        public object CachedValue { get; set; }

        internal DataStorageElement() { }

        internal DataStorageElement(DataStorageElementContext context)
        {
            Context = context;
        }

        public static DataStorageElement operator ++(DataStorageElement a)
        {
            return new DataStorageElement { Method = "add", Value = 1, Context = a.Context };
        }

        public static DataStorageElement operator --(DataStorageElement a)
        {
            return new DataStorageElement { Method = "add", Value = -1, Context = a.Context };
        }

        public static DataStorageElement operator +(DataStorageElement a, int b)
        {
            return new DataStorageElement { Method = "add", Value = b, Context = a.Context };
        }

        public static DataStorageElement operator +(DataStorageElement a, long b)
        {
            return new DataStorageElement { Method = "add", Value = b, Context = a.Context };
        }

        public static DataStorageElement operator +(DataStorageElement a, decimal b)
        {
            return new DataStorageElement { Method = "add", Value = b, Context = a.Context };
        }

        public static DataStorageElement operator +(DataStorageElement a, double b)
        {
            return new DataStorageElement { Method = "add", Value = b, Context = a.Context };
        }

        public static DataStorageElement operator +(DataStorageElement a, float b)
        {
            return new DataStorageElement { Method = "add", Value = b, Context = a.Context };
        }

        public static DataStorageElement operator +(DataStorageElement a, string b)
        {
            return new DataStorageElement { Method = "add", Value = b, Context = a.Context };
        }

        public static DataStorageElement operator -(DataStorageElement a, int b)
        {
            return new DataStorageElement { Method = "add", Value = -b, Context = a.Context };
        }

        public static DataStorageElement operator -(DataStorageElement a, long b)
        {
            return new DataStorageElement { Method = "add", Value = -b, Context = a.Context };
        }

        public static DataStorageElement operator -(DataStorageElement a, decimal b)
        {
            return new DataStorageElement { Method = "add", Value = -b, Context = a.Context };
        }

        public static DataStorageElement operator -(DataStorageElement a, double b)
        {
            return new DataStorageElement { Method = "add", Value = -b, Context = a.Context };
        }

        public static DataStorageElement operator -(DataStorageElement a, float b)
        {
            return new DataStorageElement { Method = "add", Value = -b, Context = a.Context };
        }

        public static DataStorageElement operator *(DataStorageElement a, int b)
        {
            return new DataStorageElement { Method = "mul", Value = b, Context = a.Context };
        }

        public static DataStorageElement operator *(DataStorageElement a, long b)
        {
            return new DataStorageElement { Method = "mul", Value = b, Context = a.Context };
        }

        public static DataStorageElement operator *(DataStorageElement a, decimal b)
        {
            return new DataStorageElement { Method = "mul", Value = b, Context = a.Context };
        }

        public static DataStorageElement operator *(DataStorageElement a, double b)
        {
            return new DataStorageElement { Method = "mul", Value = b, Context = a.Context };
        }

        public static DataStorageElement operator *(DataStorageElement a, float b)
        {
            return new DataStorageElement { Method = "mul", Value = b, Context = a.Context };
        }

        public static DataStorageElement operator /(DataStorageElement a, int b)
        {
            return new DataStorageElement { Method = "mul", Value = 1m / b, Context = a.Context };
        }

        public static DataStorageElement operator /(DataStorageElement a, long b)
        {
            return new DataStorageElement { Method = "mul", Value = 1m / b, Context = a.Context };
        }

        public static DataStorageElement operator /(DataStorageElement a, decimal b)
        {
            return new DataStorageElement { Method = "mul", Value = 1m / b, Context = a.Context };
        }

        public static DataStorageElement operator /(DataStorageElement a, double b)
        {
            return new DataStorageElement { Method = "mul", Value = 1d / b, Context = a.Context };
        }

        public static DataStorageElement operator /(DataStorageElement a, float b)
        {
            return new DataStorageElement { Method = "mul", Value = 1d / b, Context = a.Context };
        }

        public static DataStorageElement operator >>(DataStorageElement a, int b)
        {
            return new DataStorageElement { Method = "min", Value = b, Context = a.Context };
        }

        public static DataStorageElement operator <<(DataStorageElement a, int b)
        {
            return new DataStorageElement { Method = "max", Value = b, Context = a.Context };
        }

        public static implicit operator DataStorageElement(int i)
        {
            return new DataStorageElement { Value = i };
        }

        public static implicit operator DataStorageElement(long l)
        {
            return new DataStorageElement { Value = l };
        }

        public static implicit operator DataStorageElement(decimal l)
        {
            return new DataStorageElement { Value = l };
        }

        public static implicit operator DataStorageElement(double l)
        {
            return new DataStorageElement { Value = l };
        }

        public static implicit operator DataStorageElement(float l)
        {
            return new DataStorageElement { Value = l };
        }

        public static implicit operator DataStorageElement(string s)
        {
            return new DataStorageElement { Value = s };
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
            if (e.CachedValue == null)
            {
                e.CachedValue = e.Context.GetData(e.Context.Key);
            }

            switch (e.Method)
            {
                case "add":
                    e.CachedValue = Convert.ToString(e.CachedValue) + Convert.ToString(e.Value);
                    break;

                case "mul":
                case "max":
                case "min":
                    throw new InvalidOperationException($"Cannot perform operation {e.Method} on string value");
            }

            return Convert.ToString(e.CachedValue);
        }

        private static T RetrieveAndReturnValue<T>(DataStorageElement e) where T : struct
        {
            if (e.CachedValue != null)
            {
                return (T)e.CachedValue;
            }

            decimal value;
            decimal serverValue = Convert.ToDecimal(e.Context.GetData(e.Context.Key));

            switch (e.Method)
            {
                case "add":
                    value = serverValue + Convert.ToDecimal(e.Value);
                    break;

                case "mul":
                    value = serverValue * Convert.ToDecimal(e.Value);
                    break;

                case "max":
                    value = Math.Max(serverValue, Convert.ToDecimal(e.Value));
                    break;

                case "min":
                    value = Math.Min(serverValue, Convert.ToDecimal(e.Value));
                    break;

                default:
                    value = serverValue;
                    break;
            }

            e.CachedValue = value;

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}