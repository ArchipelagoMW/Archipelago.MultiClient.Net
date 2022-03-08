using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.Models
{
    public class DataStorageElement
    {
        /// <summary>
        /// Event handler will be called when the server side value for this key changes
        /// </summary>
        public event DataStorageHelper.DataStorageUpdatedHandler OnValueChanged
        {
            add => Context.AddHandler(Context.Key, value);
            remove => Context.RemoveHandler(Context.Key, value);
        }

        internal DataStorageElementContext Context;
        internal List<OperationSpecification> Operations = new List<OperationSpecification>(0);
        internal DataStorageHelper.DataStorageUpdatedHandler Callbacks;

        private JToken cachedValue;

        internal DataStorageElement(DataStorageElementContext context)
        {
            Context = context;
        }
        internal DataStorageElement(Operation operation, JToken value)
        {
            Operations = new List<OperationSpecification>(1) {
                new OperationSpecification { Operation = operation, Value = value }
            };
        }
        internal DataStorageElement(DataStorageElement source, Operation operation, JToken value) : this(source.Context)
        {
            Operations = source.Operations.ToList();
            Operations.Add(new OperationSpecification { Operation = operation, Value = value });
            Callbacks = source.Callbacks;
        }
        internal DataStorageElement(DataStorageElement source, Callback callback) : this(source.Context)
        {
            Operations = source.Operations.ToList();
            Callbacks = source.Callbacks;
            Callbacks += callback.Method;
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

        public static DataStorageElement operator +(DataStorageElement a, Callback c)
        {
            return new DataStorageElement(a, c);
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

        public static implicit operator DataStorageElement(List<object> l)
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
            return RetrieveAndReturnStringValue(e);
        }

        public static implicit operator JToken(DataStorageElement e)
        {
            return e.Context.GetData(e.Context.Key);
        }

        public static implicit operator bool[](DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<bool[]>(e);
        }

        public static implicit operator int[](DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<int[]>(e);
        }

        public static implicit operator long[](DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<long[]>(e);
        }

        public static implicit operator decimal[](DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<decimal[]>(e);
        }

        public static implicit operator double[](DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<double[]>(e);
        }

        public static implicit operator float[](DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<float[]>(e);
        }

        public static implicit operator string[](DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<string[]>(e);
        }

        public static implicit operator object[](DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<object[]>(e);
        }

        public static implicit operator List<bool>(DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<List<bool>>(e);
        }

        public static implicit operator List<int>(DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<List<int>>(e);
        }

        public static implicit operator List<long>(DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<List<long>>(e);
        }

        public static implicit operator List<decimal>(DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<List<decimal>>(e);
        }

        public static implicit operator List<double>(DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<List<double>>(e);
        }

        public static implicit operator List<float>(DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<List<float>>(e);
        }

        public static implicit operator List<string>(DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<List<string>>(e);
        }

        public static implicit operator List<object>(DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<List<object>>(e);
        }

        public static implicit operator JArray(DataStorageElement e)
        {
            return RetrieveAndReturnArrayValue<JArray>(e);
        }

        private static T RetrieveAndReturnArrayValue<T>(DataStorageElement e)
        {
            if (e.cachedValue != null)
            {
                return ((JArray)e.cachedValue).ToObject<T>();
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

            return value.ToObject<T>();
        }

        private static string RetrieveAndReturnStringValue(DataStorageElement e)
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

                    case Operation.Xor:
                        value = (long)value ^ (long)operation.Value;
                        break;

                    case Operation.Or:
                        value = (long)value | (long)operation.Value;
                        break;

                    case Operation.And:
                        value = (long)value & (long)operation.Value;
                        break;

                    case Operation.LeftShift:
                        value = (long)value << (int)operation.Value;
                        break;
                    
                    case Operation.RightShift:
                        value = (long)value >> (int)operation.Value;
                        break;
                }
            }

            e.cachedValue = value;

            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Retrieves the value from the server and casts it to the given type
        /// Cannot be used in combination with other operators
        /// </summary>
        /// <returns>The value from server as the given type</returns>
        /// <exception cref="T:System.InvalidOperationException">
        ///     DataStorageElement.To() cannot be used together with other operations on the DataStorageElement
        ///     Other operations include =, +=, /=, + etc
        /// </exception>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive
        /// </exception>
        public T To<T>()
        {
            if (Operations.Count != 0)
            {
                throw new InvalidOperationException(
                    "DataStorageElement.To<T>() cannot be used together with other operations on the DataStorageElement");
            }

            return Context.GetData(Context.Key).ToObject<T>();
        }

        public override string ToString()
        {
            return $"{Context?.ToString() ?? "(null)"}, ({ListOperations()})";
        }

        private string ListOperations()
        {
            if (Operations == null)
            {
                return "none";
            }

            return string.Join(", ", Operations.Select(o => o.ToString()).ToArray());
        }
    }
}