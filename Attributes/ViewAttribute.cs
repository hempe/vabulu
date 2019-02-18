using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Vabulu.Middleware;

namespace Vabulu.Attributes
{
    public class ViewAttribute : Attribute
    {
        public string TableName { get; private set; }

        public ViewAttribute(string tableName)
        {
            this.TableName = tableName;
        }

        public ITableEntity BeforeQuery(ITableEntity entity)
        {
            if (entity == null)
                return entity;
            var type = entity.GetType();
            var rowKey = this.RowKey(type);
            var partitionKey = this.PartitionKey(type);

            if (!string.IsNullOrWhiteSpace(partitionKey) && type.GetProperty(partitionKey).CanRead)
                entity.PartitionKey = (string)type.GetProperty(partitionKey).GetValue(entity);

            if (!string.IsNullOrWhiteSpace(rowKey) && type.GetProperty(rowKey).CanRead)
                entity.RowKey = (string)type.GetProperty(rowKey).GetValue(entity);

            return entity;
        }

        public ITableEntity AfterLoad(ITableEntity entity)
        {
            if (entity == null)
                return entity;
            var type = entity.GetType();
            var rowKey = this.RowKey(type);
            var partitionKey = this.PartitionKey(type);

            if (!string.IsNullOrWhiteSpace(partitionKey) && type.GetProperty(partitionKey).CanWrite)
                type.GetProperty(partitionKey).SetValue(entity, entity.PartitionKey);

            if (!string.IsNullOrWhiteSpace(rowKey) && type.GetProperty(rowKey).CanWrite)
                type.GetProperty(rowKey).SetValue(entity, entity.RowKey);

            foreach (var p in type.GetProperties())
            {
                var jsonData = p.GetCustomAttribute<JsonDataAttribute>();
                if (jsonData != null)
                {
                    p.SetValue(entity, ToObject(p.PropertyType, (string)type.GetProperty(jsonData.PropertyName).GetValue(entity)));
                }
            }

            return entity;
        }

        public string PropertyName(Type type, string propertyName)
        {
            var rowKey = this.RowKey(type);
            var partitionKey = this.PartitionKey(type);

            if (string.Equals(partitionKey, propertyName))
                return nameof(ITableEntity.PartitionKey);

            if (string.Equals(rowKey, propertyName))
                return nameof(ITableEntity.RowKey);

            var jsonData = type.GetCustomAttribute<JsonDataAttribute>();
            if (jsonData != null)
            {
                if (type.GetCustomAttribute<IgnorePropertyAttribute>() == null)
                    throw new NotSupportedException("JsonData properties must be ignored");
            }

            return propertyName;
        }

        protected string PartitionKey(Type type)
        {
            var prop = type.GetProperties().Where(p => p.GetCustomAttributes(typeof(PartitionKeyAttribute), true).Any()).FirstOrDefault();
            return prop?.Name;
        }

        protected string RowKey(Type type)
        {
            var prop = type.GetProperties().Where(p => p.GetCustomAttributes(typeof(RowKeyAttribute), true).Any()).FirstOrDefault();
            return prop?.Name;
        }

        protected static string ToJson(object value)
        {
            return value == null ? null : JsonConvert.SerializeObject(value);
        }

        protected static object ToObject(Type type, string value)
        {
            try
            {
                return string.IsNullOrEmpty(value) ? default(object) : JsonConvert.DeserializeObject(value, type);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return default(object);
            }
        }
    }
}