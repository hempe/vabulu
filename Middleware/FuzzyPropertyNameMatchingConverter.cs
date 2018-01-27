using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vabulu.Middleware {
    public class FuzzyPropertyNameMatchingConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType.Assembly == this.GetType().Assembly && objectType.GetConstructor(Type.EmptyTypes) != null;
        }

        public override bool CanWrite {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var constructor = objectType.GetConstructor(Type.EmptyTypes);

            var instance = constructor.Invoke(null);
            var props = objectType.GetProperties().Select(x => new {
                Name = this.Simplify(x.Name),
                    Property = x
            }).ToArray();

            if (IsSimple(reader.ValueType)) {
                return reader.Value;
            }

            var jo = JObject.Load(reader);
            foreach (JProperty jp in jo.Properties()) {
                var name = this.Simplify(jp.Name);
                var prop = props.FirstOrDefault(pi =>
                    pi.Property.CanWrite && string.Equals(pi.Name, name, StringComparison.OrdinalIgnoreCase));

                if (prop != null)
                    if (prop.Property.PropertyType.IsArray && !(jp.Value is JArray)) {
                        this.ParseDictionaryAsListByType(
                            prop.Property.PropertyType.GetElementType(),
                            jp,
                            serializer,
                            prop.Property,
                            instance
                        );
                    } else {
                        prop.Property.SetValue(instance, jp.Value.ToObject(prop.Property.PropertyType, serializer));
                    }
            }
            return instance;
        }

        private bool IsSimple(Type type) {
            if (type == null)
                return false;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                return IsSimple(type.GetGenericArguments() [0]);
            }
            return type.IsPrimitive ||
                type.IsEnum ||
                type.Equals(typeof(string)) ||
                type.Equals(typeof(decimal));
        }

        private void ParseDictionaryAsListByType(Type elementType, JProperty jp, JsonSerializer serializer, PropertyInfo prop, object instance) {
            this.GetType()
                .GetMethod(nameof(ParseDictionaryAsList), BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(elementType)
                .Invoke(this, new [] { jp, serializer, prop, instance });
        }
        private void ParseDictionaryAsList<TElement>(JProperty jp, JsonSerializer serializer, PropertyInfo prop, object instance) {
            var surogateValue = (Dictionary<string, object>) jp.Value.ToObject(typeof(Dictionary<string, object>), serializer);
            var value = surogateValue
                .Where(kv => int.TryParse(kv.Key.ToString(), out var _))
                .Select(x => x.Value as JObject)
                .Where(x => x != null)
                .Select(x => x.ToObject<TElement>())
                .ToArray();
            prop.SetValue(instance, value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotImplementedException();

        public bool IsGenericArray(Type type) => type.IsGenericType && type.IsArray;

        private string Simplify(string name) => Regex.Replace(name, "[^A-Za-z0-9]+", "").ToLower();
    }
}