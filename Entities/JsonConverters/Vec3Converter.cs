using Newtonsoft.Json;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ARInventory.Entities.JsonConverters
{
    internal class Vec3Converter : JsonConverter<Vec3>
    {
        public override Vec3 ReadJson(JsonReader reader, Type objectType, Vec3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // idk if this is correct... but it's not being used at the moment
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, Vec3 value, JsonSerializer serializer)
        {
            Vector3 v = value.v;
            serializer.Serialize(writer, v);
        }
    }
}
