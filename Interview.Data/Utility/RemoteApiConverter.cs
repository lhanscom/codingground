using System;
using Newtonsoft.Json;

namespace Interview.Data.Utility
{
    public class RemoteApiConverter : JsonConverter
    {

        public override bool CanConvert(System.Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new System.NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            
            if (value is int)
            {
                writer.WriteValue(value);
            }
            else
            {
                if (value is DateTime)
                {
                    var dt = (DateTime?)value;
                    writer.WriteValue(dt.Value.GetDateForApi());
                }
                else
                {
                    writer.WriteValue(value.ToString().GetStringForApi());
                }                
            }
            writer.Flush();
        }
    }
}