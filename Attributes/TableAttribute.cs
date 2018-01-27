using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Vabulu.Middleware;

namespace Vabulu.Attributes {
    public class TableAttribute : ViewAttribute {

        public TableAttribute(string name) : base(name) { }

        public ITableEntity BeforeSave(ITableEntity entity) {
            if (this.BeforeQuery(entity) == null)
                return entity;
            var type = entity.GetType();
            foreach (var p in type.GetProperties()) {
                var jsonData = p.GetCustomAttributes(typeof(JsonDataAttribute), true).Cast<JsonDataAttribute>().FirstOrDefault();
                if (jsonData == null)
                    continue;
                type.GetProperty(jsonData.PropertyName).SetValue(entity, ToJson(p.GetValue(entity)));
            }

            return entity;
        }

    }
}