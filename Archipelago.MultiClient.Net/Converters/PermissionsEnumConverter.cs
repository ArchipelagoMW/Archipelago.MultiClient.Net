using Archipelago.MultiClient.Net.Enums;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using System;

namespace Archipelago.MultiClient.Net.Converters
{
    public class PermissionsEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string) || objectType == typeof(Permissions) || objectType == typeof(int);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value.ToString();
            var isInt = int.TryParse(value, out var intValue);

            if (isInt)
            {
                return (Permissions)intValue;
            }

            var returnValue = Permissions.Disabled;

            if (value.Contains("enabled"))
            {
                returnValue |= Permissions.Enabled;
            }

            if (value.Contains("auto"))
            {
                returnValue |= Permissions.Auto;
            }

            if (value.Contains("goal"))
            {
                returnValue |= Permissions.Goal;
            }

            return returnValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var permissionsValue = (Permissions)value;

            writer.WriteValue((int)permissionsValue);
        }
    }
}
