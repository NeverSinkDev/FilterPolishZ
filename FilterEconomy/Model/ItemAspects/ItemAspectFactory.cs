using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FilterEconomy.Model.ItemAspects
{
    public class ItemAspectFactory : JsonConverter
    {

        public Dictionary<string, string> MigrationList = new Dictionary<string, string>
        {
           { "IgnoreAspect", "AnchorAspect" },
           { "PoorDiviAspect", "PoorDropAspect" },
           { "FloorFragmentsAspect", "PreventHidingAspect" },
           { "MetaBiasAspect", "REMOVE"}
        };

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

            IItemAspect targetObj;
            if (!MigrationList.ContainsKey(jsonObj.Name))
            {
                targetObj = ItemAspectFactory.CreateAspectFromString(jsonObj.Name);
                JsonConvert.PopulateObject(json, targetObj);
            }
            else
            {
                if (MigrationList[jsonObj.Name] == "REMOVE")
                {
                    return null;
                }
                else
                {
                    targetObj = ItemAspectFactory.CreateAspectFromString(MigrationList[jsonObj.Name]);
                }
            }

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