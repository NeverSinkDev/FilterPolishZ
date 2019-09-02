using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FilterEconomy.Model.ItemAspects
{
    public class ItemAspectFactory : JsonConverter
    {
        private static IItemAspect CreateAspectFromString(string className)
        {
            var asm = System.Reflection.Assembly.GetAssembly(typeof(IItemAspect));

            foreach (var objClass in asm.GetTypes())
            {
                if (objClass.Name != className) continue;
                
                var res = (IItemAspect) objClass.GetConstructor(Type.EmptyTypes)?.Invoke(null);
                return res;
            }
            
            throw new Exception("class not found");
        }
            
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var aspect = (IItemAspect) value;
            var json = JsonConvert.SerializeObject(aspect);
            writer.WriteValue(json);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var json = reader.Value.ToString();
            var jsonObj = JsonConvert.DeserializeObject<EmptyAspect>(json, new JsonSerializerSettings() { CheckAdditionalContent = true });
            var targetObj = ItemAspectFactory.CreateAspectFromString(jsonObj.Name);
            JsonConvert.PopulateObject(json, targetObj);
            return targetObj;
        }

        public override bool CanConvert(Type objectType)
        {
            var isAspect = objectType.GetInterfaces().Contains(typeof(IItemAspect));
            isAspect = isAspect || typeof(IItemAspect) == objectType;
            return isAspect;
        }
        
        private class EmptyAspect { public string Name { get; set; } }
    }
}