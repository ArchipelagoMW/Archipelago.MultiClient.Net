using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json.Linq;
#else
using Newtonsoft.Json.Linq;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if !NET35
using System.Threading.Tasks;
#endif

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

        JToken cachedValue;

        internal DataStorageElement(DataStorageElementContext context)
        {
            Context = context;
        }
#if !USE_OCULUS_NEWTONSOFT
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
#else
        internal DataStorageElement(string operation, JToken value)
        {
            Operations = new List<OperationSpecification>(1) {
                new OperationSpecification { Operation = operation, Value = value }
            };
        }

        internal DataStorageElement(DataStorageElement source, string operation, JToken value) : this(source.Context)
        {
            Operations = source.Operations.ToList();
            Operations.Add(new OperationSpecification { Operation = operation, Value = value });
            Callbacks = source.Callbacks;
        }
#endif
        internal DataStorageElement(DataStorageElement source, Callback callback) : this(source.Context)
        {
            Operations = source.Operations.ToList();
            Callbacks = source.Callbacks;
            Callbacks += callback.Method;
        }

        public static DataStorageElement operator ++(DataStorageElement a)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Add, 1);
#else
            return new DataStorageElement(a, Operation.Add.ToString(), 1);
#endif
        }

        public static DataStorageElement operator --(DataStorageElement a)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Add, -1);
#else
            return new DataStorageElement(a, Operation.Add.ToString(), -1);
#endif
        }

        public static DataStorageElement operator +(DataStorageElement a, JToken b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Add, b);
#else
            return new DataStorageElement(a, Operation.Add.ToString(), b);
#endif
        }

        public static DataStorageElement operator +(DataStorageElement a, IEnumerable b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Add, JArray.FromObject(b));
#else
            return new DataStorageElement(a, Operation.Add.ToString(), JArray.FromObject(b));
#endif
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
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Mul, b);
#else
            return new DataStorageElement(a, Operation.Mul.ToString(), b);
#endif
        }

        public static DataStorageElement operator %(DataStorageElement a, JToken b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Mod, b);
#else
            return new DataStorageElement(a, Operation.Mod.ToString(), b);
#endif
        }

        public static DataStorageElement operator ^(DataStorageElement a, JToken b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Pow, b);
#else
            return new DataStorageElement(a, Operation.Pow.ToString(), b);
#endif            
        }

        public static DataStorageElement operator -(DataStorageElement a, int b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Add, JToken.FromObject(-b));
#else
            return new DataStorageElement(a, Operation.Add.ToString(), JToken.FromObject(-b));
#endif
        }

        public static DataStorageElement operator -(DataStorageElement a, long b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Add, JToken.FromObject(-b));
#else
            return new DataStorageElement(a, Operation.Add.ToString(), JToken.FromObject(-b));
#endif
        }

        public static DataStorageElement operator -(DataStorageElement a, decimal b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Add, JToken.FromObject(-b));
#else
            return new DataStorageElement(a, Operation.Add.ToString(), JToken.FromObject(-b));
#endif
        }

        public static DataStorageElement operator -(DataStorageElement a, double b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Add, JToken.FromObject(-b));
#else
            return new DataStorageElement(a, Operation.Add.ToString(), JToken.FromObject(-b));
#endif
        }

        public static DataStorageElement operator -(DataStorageElement a, float b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Add, JToken.FromObject(-b));
#else
            return new DataStorageElement(a, Operation.Add.ToString(), JToken.FromObject(-b));
#endif
        }

        public static DataStorageElement operator /(DataStorageElement a, int b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Mul, JToken.FromObject(1m / b));
#else
            return new DataStorageElement(a, Operation.Mul.ToString(), JToken.FromObject(1m / b));
#endif
        }

        public static DataStorageElement operator /(DataStorageElement a, long b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Mul, JToken.FromObject(1m / b));
#else
            return new DataStorageElement(a, Operation.Mul.ToString(), JToken.FromObject(1m / b));
#endif
        }

        public static DataStorageElement operator /(DataStorageElement a, decimal b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Mul, JToken.FromObject(1m / b));
#else
            return new DataStorageElement(a, Operation.Mul.ToString(), JToken.FromObject(1m / b));
#endif
        }

        public static DataStorageElement operator /(DataStorageElement a, double b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Mul, JToken.FromObject(1d / b));
#else
            return new DataStorageElement(a, Operation.Mul.ToString(), JToken.FromObject(1d / b));
#endif
        }

        public static DataStorageElement operator /(DataStorageElement a, float b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Mul, JToken.FromObject(1d / b));
#else
            return new DataStorageElement(a, Operation.Mul.ToString(), JToken.FromObject(1d / b));
#endif
        }

        public static DataStorageElement operator >>(DataStorageElement a, int b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Min, b);
#else
            return new DataStorageElement(a, Operation.Min.ToString(), b);
#endif
        }

        public static DataStorageElement operator <<(DataStorageElement a, int b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(a, Operation.Max, b);
#else
            return new DataStorageElement(a, Operation.Max.ToString(), b);
#endif
        }

        public static implicit operator DataStorageElement(bool b)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, b);
#else
            return new DataStorageElement(Operation.Replace.ToString(), b);
#endif
        }

        public static implicit operator DataStorageElement(int i)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, i);
#else
            return new DataStorageElement(Operation.Replace.ToString(), i);
#endif
        }

        public static implicit operator DataStorageElement(long l)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, l);
#else
            return new DataStorageElement(Operation.Replace.ToString(), l);
#endif
        }

        public static implicit operator DataStorageElement(decimal m)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, m);
#else
            return new DataStorageElement(Operation.Replace.ToString(), m);
#endif
        }

        public static implicit operator DataStorageElement(double d)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, d);
#else
            return new DataStorageElement(Operation.Replace.ToString(), d);
#endif
        }

        public static implicit operator DataStorageElement(float f)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, f);
#else
            return new DataStorageElement(Operation.Replace.ToString(), f);
#endif
        }

        public static implicit operator DataStorageElement(string s)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, s);
#else
            return new DataStorageElement(Operation.Replace.ToString(), s);
#endif
        }

        public static implicit operator DataStorageElement(JToken o)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, o);
#else
            return new DataStorageElement(Operation.Replace.ToString(), o);
#endif
        }

        public static implicit operator DataStorageElement(Array a)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, JArray.FromObject(a));
#else
            return new DataStorageElement(Operation.Replace.ToString(), JArray.FromObject(a));
#endif
        }

        public static implicit operator DataStorageElement(List<bool> l)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
#else
            return new DataStorageElement(Operation.Replace.ToString(), JArray.FromObject(l));
#endif
        }

        public static implicit operator DataStorageElement(List<int> l)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
#else
            return new DataStorageElement(Operation.Replace.ToString(), JArray.FromObject(l));
#endif
        }

        public static implicit operator DataStorageElement(List<long> l)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
#else
            return new DataStorageElement(Operation.Replace.ToString(), JArray.FromObject(l));
#endif
        }

        public static implicit operator DataStorageElement(List<decimal> l)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
#else
            return new DataStorageElement(Operation.Replace.ToString(), JArray.FromObject(l));
#endif
        }

        public static implicit operator DataStorageElement(List<double> l)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
#else
            return new DataStorageElement(Operation.Replace.ToString(), JArray.FromObject(l));
#endif
        }

        public static implicit operator DataStorageElement(List<float> l)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
#else
            return new DataStorageElement(Operation.Replace.ToString(), JArray.FromObject(l));
#endif
        }

        public static implicit operator DataStorageElement(List<string> l)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
#else
            return new DataStorageElement(Operation.Replace.ToString(), JArray.FromObject(l));
#endif
        }

        public static implicit operator DataStorageElement(List<object> l)
        {
#if !USE_OCULUS_NEWTONSOFT
            return new DataStorageElement(Operation.Replace, JArray.FromObject(l));
#else
            return new DataStorageElement(Operation.Replace.ToString(), JArray.FromObject(l));
#endif
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

        /// <summary>
        /// Initializes a value in the server side data storage
        /// Will not override any existing value, only set the default value if none existed
        /// </summary>
        /// <param name="value">The default value for the key</param>
        public void Initialize(JToken value) => Context.Initialize(Context.Key, value);

		/// <summary>
		/// Initializes a value in the server side data storage
		/// Will not override any existing value, only set the default value if none existed
		/// </summary>
		/// <param name="value">The default value for the key</param>
		public void Initialize(IEnumerable value) => Context.Initialize(Context.Key, JArray.FromObject(value));

#if NET35
        /// <summary>
        /// Retrieves the value of a certain key from server side data storage.
        /// </summary>
        /// <param name="callback">The callback that will be called when the value is retrieved</param>
        public void GetAsync<T>(Action<T> callback) => GetAsync(t => callback(t.ToObject<T>()));

        /// <summary>
        /// Retrieves the value of a certain key from server side data storage.
        /// </summary>
        /// <param name="callback">The callback that will be called when the value is retrieved</param>
        public void GetAsync(Action<JToken> callback) => Context.GetAsync(Context.Key, callback);
#else
        /// <summary>
        /// Retrieves the value of a certain key from server side data storage.
        /// </summary>
        public Task<T> GetAsync<T>() => GetAsync().ContinueWith(r => r.Result.ToObject<T>());

        /// <summary>
        /// Retrieves the value of a certain key from server side data storage.
        /// </summary>
        public Task<JToken> GetAsync() => Context.GetAsync(Context.Key);
#endif

        static T RetrieveAndReturnArrayValue<T>(DataStorageElement e)
        {
            if (e.cachedValue != null)
                return ((JArray)e.cachedValue).ToObject<T>();

            var value = e.Context.GetData(e.Context.Key).ToObject<JArray>() ?? new JArray();

            foreach (var operation in e.Operations)
            {
                switch (operation.Operation)
                {
#if USE_OCULUS_NEWTONSOFT
                    case "Add":
#else
                    case Operation.Add:
#endif
                        if (operation.Value.Type != JTokenType.Array)
                            throw new InvalidOperationException(
                                $"Cannot perform operation {Operation.Add} on Array value, with a non Array value: {operation.Value}");

                        value.Merge(operation.Value);
                        break;


#if USE_OCULUS_NEWTONSOFT
                    case "Replace":
#else
                    case Operation.Replace:
#endif
                        if (operation.Value.Type != JTokenType.Array)
                            throw new InvalidOperationException($"Cannot replace Array value, with a non Array value: {operation.Value}");

                        value = operation.Value.ToObject<JArray>() ?? new JArray();
                        break;

                    default:
                        throw new InvalidOperationException($"Cannot perform operation {operation.Operation} on Array value");
                }
            }

            e.cachedValue = value;

            return value.ToObject<T>();
        }

        static string RetrieveAndReturnStringValue(DataStorageElement e)
        {
            if (e.cachedValue != null)
                return (string)e.cachedValue;

            var value = e.Context.GetData(e.Context.Key).ToString();

            foreach (var operation in e.Operations)
            {
                switch (operation.Operation)
                {
#if USE_OCULUS_NEWTONSOFT
                    case "Add":
#else
                    case Operation.Add:
#endif
                        value += (string)operation.Value;
                        break;

#if USE_OCULUS_NEWTONSOFT
                    case "Mul":
#else
                    case Operation.Mul:
#endif
                        if (operation.Value.Type != JTokenType.Integer)
                            throw new InvalidOperationException($"Cannot perform operation {Operation.Mul} on string value, with a non interger value: {operation.Value}");

                        value = string.Concat(Enumerable.Repeat(value, (int)operation.Value));
                        break;

#if USE_OCULUS_NEWTONSOFT
                    case "Replace":
#else
                    case Operation.Replace:
#endif
                        value = (string)operation.Value;
                        break;

                    default:
                        throw new InvalidOperationException($"Cannot perform operation {operation.Operation} on string value");
                }
            }

            e.cachedValue = value;

            return Convert.ToString(e.cachedValue);
        }

        static T RetrieveAndReturnDecimalValue<T>(DataStorageElement e) where T : struct
        {
            if (e.cachedValue != null)
                return e.cachedValue.ToObject<T>();

            var value = e.Context.GetData(e.Context.Key).ToObject<decimal>();

            foreach (var operation in e.Operations)
            {
               switch (operation.Operation)
               {
#if USE_OCULUS_NEWTONSOFT
                    case "Replace":
#else
                    case Operation.Replace:
#endif
                        value = (decimal)operation.Value;
                        break;

#if USE_OCULUS_NEWTONSOFT
                    case "Add":
#else
                    case Operation.Add:
#endif
                        value += (decimal)operation.Value;
                        break;

#if USE_OCULUS_NEWTONSOFT
                    case "Mul":
#else
                    case Operation.Mul:
#endif
                        value *= (decimal)operation.Value;
                        break;

#if USE_OCULUS_NEWTONSOFT
                    case "Mod":
#else
                    case Operation.Mod:
#endif
                        value %= (decimal)operation.Value;
                        break;

#if USE_OCULUS_NEWTONSOFT
                    case "Pow":
#else
                    case Operation.Pow:
#endif
                        value = (decimal)Math.Pow((double)value, (double)operation.Value);
                        break;

#if USE_OCULUS_NEWTONSOFT
                    case "Max":
#else
                    case Operation.Max:
#endif
                        value = Math.Max(value, (decimal)operation.Value);
                        break;

#if USE_OCULUS_NEWTONSOFT
                    case "Min":
#else
                    case Operation.Min:
#endif
                        value = Math.Min(value, (decimal)operation.Value);
                        break;

#if USE_OCULUS_NEWTONSOFT
                    case "Xor":
#else
                    case Operation.Xor:
#endif
                        value = (long)value ^ (long)operation.Value;
                        break;

#if USE_OCULUS_NEWTONSOFT
                    case "Or":
#else
                    case Operation.Or:
#endif
                        value = (long)value | (long)operation.Value;
                        break;

#if USE_OCULUS_NEWTONSOFT
                    case "And":
#else
                    case Operation.And:
#endif
                        value = (long)value & (long)operation.Value;
                        break;

#if USE_OCULUS_NEWTONSOFT
                    case "left_shift":
#else
                    case Operation.LeftShift:
#endif
                        value = (long)value << (int)operation.Value;
                        break;

#if USE_OCULUS_NEWTONSOFT
                    case "right_shift":
#else
                    case Operation.RightShift:
#endif
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
                throw new InvalidOperationException(
                    "DataStorageElement.To<T>() cannot be used together with other operations on the DataStorageElement");

            return Context.GetData(Context.Key).ToObject<T>();
        }

        public override string ToString() => $"{Context?.ToString() ?? "(null)"}, ({ListOperations()})";

        string ListOperations() => Operations == null 
	        ? "none" 
	        : string.Join(", ", Operations.Select(o => o.ToString()).ToArray());
    }
}