using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.Models
{
    public class DataStorageElement : IEnumerable
    {
        public event DataStorageHelper.DataStorageUpdatedHandler OnValueChanged
        {
            add => Context.AddHandler(Context.Key, value);
            remove => Context.RemoveHandler(Context.Key, value);
        }

        internal DataStorageElementContext Context;
        internal List<OperationSpecification> Operations;

        private JToken cachedValue;

        internal DataStorageElement(DataStorageElementContext context)
        {
            Context = context;
            Operations = new List<OperationSpecification>(0);
        }

        internal DataStorageElement(Operation operation, JToken value)
        {
            Operations = new List<OperationSpecification>(1) {
                new OperationSpecification { Operation = operation, Value = value }
            };
        }

        internal DataStorageElement(DataStorageElement source, Operation operation, JToken value)
            : this(source, new OperationSpecification { Operation = operation, Value = value })
        {
        }

        internal DataStorageElement(DataStorageElement source, OperationSpecification operation) 
            : this(source.Context)
        {
            Operations = new List<OperationSpecification>(source.Operations.Count + 1);
            Operations.AddRange(source.Operations);
            Operations.Add(operation);
        }

        public static DataStorageElement operator ++(DataStorageElement a)
        {
            return new DataStorageElement(a, Operation.Add, 1);
        }

        public static DataStorageElement operator --(DataStorageElement a)
        {
            return new DataStorageElement(a, Operation.Add, -1);
        }

        public static DataStorageElement operator +(DataStorageElement a, JToken b)
        {
            return new DataStorageElement(a, Operation.Add, b);
        }

        public static DataStorageElement operator +(DataStorageElement a, IEnumerable b)
        {
            return new DataStorageElement(a, Operation.Add, JArray.FromObject(b));
        }

        public static DataStorageElement operator +(DataStorageElement a, OperationSpecification s)
        {
            return new DataStorageElement(a, s.Operation, s.Value);
        }

        public static DataStorageElement operator *(DataStorageElement a, JToken b)
        {
            return new DataStorageElement(a, Operation.Mul, b);
        }

        public static DataStorageElement operator %(DataStorageElement a, JToken b)
        {
            return new DataStorageElement(a, Operation.Mod, b);
        }

        public static DataStorageElement operator ^(DataStorageElement a, JToken b)
        {
            return new DataStorageElement(a, Operation.Pow, b);
        }

        public static DataStorageElement operator -(DataStorageElement a, int b)

        {
            return new DataStorageElement(a, Operation.Add, JToken.FromObject(-b));
        }

        public static DataStorageElement operator -(DataStorageElement a, long b)
        {
            return new DataStorageElement(a, Operation.Add, JToken.FromObject(-b));
        }

        public static DataStorageElement operator -(DataStorageElement a, decimal b)
        {
            return new DataStorageElement(a, Operation.Add, JToken.FromObject(-b));
        }

        public static DataStorageElement operator -(DataStorageElement a, double b)
        {
            return new DataStorageElement(a, Operation.Add, JToken.FromObject(-b));
        }

        public static DataStorageElement operator -(DataStorageElement a, float b)
        {
            return new DataStorageElement(a, Operation.Add, JToken.FromObject(-b));
        }

        public static DataStorageElement operator /(DataStorageElement a, int b)
        {
            return new DataStorageElement(a, Operation.Mul, JToken.FromObject(1m / b));
        }

        public static DataStorageElement operator /(DataStorageElement a, long b)
        {
            return new DataStorageElement(a, Operation.Mul, JToken.FromObject(1m / b));
        }

        public static DataStorageElement operator /(DataStorageElement a, decimal b)
        {
            return new DataStorageElement(a, Operation.Mul, JToken.FromObject(1m / b));
        }

        public static DataStorageElement operator /(DataStorageElement a, double b)
        {
            return new DataStorageElement(a, Operation.Mul, JToken.FromObject(1d / b));
        }

        public static DataStorageElement operator /(DataStorageElement a, float b)
        {
            return new DataStorageElement(a, Operation.Mul, JToken.FromObject(1d / b));
        }

        public static DataStorageElement operator >>(DataStorageElement a, int b)
        {
            return new DataStorageElement(a, Operation.Min, b);
        }

        public static DataStorageElement operator <<(DataStorageElement a, int b)
        {
            return new DataStorageElement(a, Operation.Max, b);
        }

        public static implicit operator DataStorageElement(bool b)
        {
            return new DataStorageElement(Operation.Replace, b);
        }

        public static implicit operator DataStorageElement(int i)
        {
            return new DataStorageElement(Operation.Replace, i);
        }

        public static implicit operator DataStorageElement(long l)
        {
            return new DataStorageElement(Operation.Replace, l);
        }

        public static implicit operator DataStorageElement(decimal m)
        {
            return new DataStorageElement(Operation.Replace, m);
        }

        public static implicit operator DataStorageElement(double d)
        {
            return new DataStorageElement(Operation.Replace, d);
        }

        public static implicit operator DataStorageElement(float f)
        {
            return new DataStorageElement(Operation.Replace, f);
        }

        public static implicit operator DataStorageElement(string s)
        {
            return new DataStorageElement(Operation.Replace, s);
        }

        public static implicit operator DataStorageElement(JToken o)
        {
            return new DataStorageElement(Operation.Replace, o);
        }

        public static implicit operator DataStorageElement(Array a)
        {
            return new DataStorageElement(Operation.Replace, JArray.FromObject(a));
        }

        public static implicit operator DataStorageElement(List<bool> l)
        {
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
        }

        public static implicit operator DataStorageElement(List<int> l)
        {
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
        }

        public static implicit operator DataStorageElement(List<long> l)
        {
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
        }

        public static implicit operator DataStorageElement(List<decimal> l)
        {
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
        }

        public static implicit operator DataStorageElement(List<double> l)
        {
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
        }

        public static implicit operator DataStorageElement(List<float> l)
        {
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
        }

        public static implicit operator DataStorageElement(List<string> l)
        {
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
        }

        public static implicit operator int(DataStorageElement e)
        {
            return RetrieveAndReturnDecimalValue<int>(e);
        }

        public static implicit operator long(DataStorageElement e)
        {
            return RetrieveAndReturnDecimalValue<long>(e);
        }

        public static implicit operator decimal(DataStorageElement e)
        {
            return RetrieveAndReturnDecimalValue<decimal>(e);
        }

        public static implicit operator double(DataStorageElement e)
        {
            return RetrieveAndReturnDecimalValue<double>(e);
        }

        public static implicit operator float(DataStorageElement e)
        {
            return RetrieveAndReturnDecimalValue<float>(e);
        }

        public static implicit operator string(DataStorageElement e)
        {
            return RetrieveAndReturnValue(e);
        }

        public static implicit operator JToken(DataStorageElement e)
        {
            return e.Context.GetData(e.Context.Key);
        }

        public static implicit operator JArray(DataStorageElement e)
        {
            if (e.cachedValue != null)
            {
                return (JArray)e.cachedValue;
            }

            var value = e.Context.GetData(e.Context.Key).ToObject<JArray>() ?? new JArray();

            foreach (var operation in e.Operations)
            {
                switch (operation.Operation)
                {
                    case Operation.Add:
                        if (operation.Value.Type != JTokenType.Array)
                        {
                            throw new InvalidOperationException(
                                $"Cannot perform operation {Operation.Add} on Array value, with a non Array value: {operation.Value}");
                        }

                        value.Merge(operation.Value);
                        break;

                    case Operation.Replace:
                        if (operation.Value.Type != JTokenType.Array)
                        {
                            throw new InvalidOperationException(
                                $"Cannot replace Array value, with a non Array value: {operation.Value}");
                        }

                        value = operation.Value.ToObject<JArray>() ?? new JArray();
                        break;

                    default:
                        throw new InvalidOperationException($"Cannot perform operation {operation.Operation} on Array value");
                }

            }

            e.cachedValue = value;

            return value;
        }

        private static string RetrieveAndReturnValue(DataStorageElement e)
        {
            if (e.cachedValue != null)
            {
                return (string)e.cachedValue;
            }

            string value = e.Context.GetData(e.Context.Key).ToString();

            foreach (var operation in e.Operations)
            {
                switch (operation.Operation)
                {
                    case Operation.Add:
                        value += (string)operation.Value;
                        break;

                    case Operation.Mul:
                        if (operation.Value.Type != JTokenType.Integer)
                        {
                            throw new InvalidOperationException($"Cannot perform operation {Operation.Mul} on string value, with a non interger value: {operation.Value}");
                        }

                        value = string.Concat(Enumerable.Repeat(value, (int)operation.Value));
                        break;

                    case Operation.Replace:
                        value = (string)operation.Value;
                        break;

                    default:
                        throw new InvalidOperationException($"Cannot perform operation {operation.Operation} on string value");
                }
            }

            e.cachedValue = value;

            return Convert.ToString(e.cachedValue);
        }

        private static T RetrieveAndReturnDecimalValue<T>(DataStorageElement e) where T : struct
        {
            if (e.cachedValue != null)
            {
                return e.cachedValue.ToObject<T>();
            }

            decimal value = e.Context.GetData(e.Context.Key).ToObject<decimal>();

            foreach (var operation in e.Operations)
            {
               switch (operation.Operation)
               {
                    case Operation.Replace:
                        value = (decimal)operation.Value;
                        break;

                    case Operation.Add:
                        value += (decimal)operation.Value;
                        break;

                    case Operation.Mul:
                        value *= (decimal)operation.Value;
                        break;
                    
                    case Operation.Mod:
                        value %= (decimal)operation.Value;
                        break;

                    case Operation.Pow:
                        value = (decimal)Math.Pow((double)value, (double)operation.Value);
                        break;

                    case Operation.Max:
                        value = Math.Max(value, (decimal)operation.Value);
                        break;

                    case Operation.Min:
                        value = Math.Min(value, (decimal)operation.Value);
                        break;
                }
            }

            e.cachedValue = value;

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public T To<T>()
        {
            return Context.GetData(Context.Key).ToObject<T>();
        }

        public override string ToString()
        {
            return $"{Context?.ToString() ?? "(null)"}, ({ListOperations()})";
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)Context.GetData(Context.Key)).GetEnumerator();
        }

        public string ListOperations()
        {
            if (Operations == null)
            {
                return "none";
            }

            return string.Join(", ", Operations.Select(o => o.ToString()).ToArray());
        }
    }
}