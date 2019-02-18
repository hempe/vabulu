using System;
using Microsoft.Azure;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Vabulu.Attributes;
using Vabulu.Middleware;
using WindowsAzure.Table;
using WindowsAzure.Table.Extensions;
using WindowsAzure.Table.Queryable;
using WindowsAzure.Table.Queryable.Base;
using WindowsAzure.Table.Queryable.Expressions;

namespace Vabulu.Services
{
    public class StoreOption
    {
        public string ConnectionString { get; set; }
        public string ImageContainer { get; set; }
        public string Prefix { get; set; }
    }

    public class Args : Dictionary<string, string> { }

    public class Args<T> : Args
    {

        public static Args<T> Where(Expression<Func<T, string>> expression, string value)
        {
            return new Args<T> { { Check.GetPropertyName(expression), value } };
        }
    }

    public static class ArgsExtensions
    {
        public static Args<T> And<T>(this Args<T> args, Expression<Func<T, string>> expression, string value)
        {
            args[Check.GetPropertyName(expression)] = value;
            return args;
        }

        public static Args As(this Args arguments, Type type)
        {
            var args = (Args)Activator.CreateInstance(typeof(Args<>).MakeGenericType(type));
            if (arguments == null)
                return args;
            foreach (var key in arguments.Keys)
            {
                if (type.GetProperty(key) == null)
                    throw new NotSupportedException($"Property {key} does not exist on type {type.Name}");
                args[key] = arguments[key];
            }
            return args;
        }

        public static Args<T> As<T>(this Args args)
        {
            return (Args<T>)args.As(typeof(T));
        }
    }

    public class TableStore
    {
        private static readonly Dictionary<Type, CloudTable> Tables;
        private static readonly Dictionary<Type, CloudTable> Views;
        private static readonly Dictionary<Type, object> TableSets;
        private static readonly Dictionary<Type, TableAttribute> TableAttributes;
        private static readonly Dictionary<Type, ViewAttribute> ViewAttributes;

        static TableStore()
        {
            Tables = new Dictionary<Type, CloudTable>();
            TableSets = new Dictionary<Type, object>();
            Views = new Dictionary<Type, CloudTable>();
            TableAttributes = new Dictionary<Type, TableAttribute>();
            ViewAttributes = new Dictionary<Type, ViewAttribute>();
        }

        private readonly CloudTableClient tableClient;
        private readonly StoreOption options;

        public TableStore(IOptions<StoreOption> options)
        {
            this.options = options.Value;
            var cloudStorageAccount = CloudStorageAccount.Parse(this.options.ConnectionString);
            this.tableClient = cloudStorageAccount.CreateCloudTableClient();
        }

        private TableAttribute GetTableAttribute(Type type) => TableAttributes.GetOrAdd(type, () => type.Table());

        private ViewAttribute GetViewAttribute(Type type) => ViewAttributes.GetOrAdd(type, () => type.View());

        private Task<CloudTable> GetViewAsync(Type type) => Views.GetOrAddAsync(type, async () =>
        {
            var view = tableClient.GetTableReference($"{this.options.Prefix}{this.GetViewAttribute(type).TableName}");
            await view.CreateIfNotExistsAsync();
            return view;
        });

        private async Task<TableSet<T>> GetViewAsync<T>() where T : class, ITableEntity, new()
        {
            var value = await TableSets.GetOrAddAsync(typeof(T), async () =>
            {
                var view = tableClient.GetTableReference($"{this.options.Prefix}{this.GetViewAttribute(typeof(T)).TableName}");
                await view.CreateIfNotExistsAsync();
                var countryTable = new TableSet<T>(tableClient, $"{this.options.Prefix}{this.GetViewAttribute(typeof(T)).TableName}");
                return (object)countryTable; ;
            });
            return (TableSet<T>)value;
        }

        private Task<CloudTable> GetTableAsync(Type type) => Views.GetOrAddAsync(type, async () =>
        {
            var view = tableClient.GetTableReference($"{this.options.Prefix}{this.GetTableAttribute(type).TableName}");
            await view.CreateIfNotExistsAsync();
            return view;
        });

        public async Task<TableResult> DeleteAsync(Type type, ITableEntity entity)
        {

            var table = await this.GetTableAsync(type);
            entity.ETag = "*";
            entity = this.GetTableAttribute(type).BeforeSave(entity);
            var tableOperation = TableOperation.Delete(entity);
            return await table.ExecuteAsync(tableOperation);
        }
        public Task<TableResult> DeleteAsync<T>(T entity) where T : class, ITableEntity, new() => this.DeleteAsync(typeof(T), entity);

        public Task<TableResult> AddOrUpdateAsync<T>(T entity) where T : class, ITableEntity, new() => this.AddOrUpdateAsync(typeof(T), entity);
        public async Task<TableResult> AddOrUpdateAsync(Type type, ITableEntity entity)
        {
            var table = await this.GetTableAsync(type);
            entity = this.GetTableAttribute(type).BeforeSave(entity);

            var tableOperation = TableOperation.InsertOrReplace(entity);
            return await table.ExecuteAsync(tableOperation);
        }

        public async Task<T> GetAsync<T>(T entity) where T : class, ITableEntity, new()
        {
            var type = typeof(T);
            var table = await this.GetViewAsync(type);
            var attribute = this.GetViewAttribute(type);
            attribute.BeforeQuery(entity);

            var tableOperation = TableOperation.Retrieve<T>(entity.PartitionKey, entity.RowKey);
            var tableResult = await table.ExecuteAsync(tableOperation);
            try
            {
                var value = tableResult.Result as T;
                attribute.AfterLoad(value);
                return value;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<T> GetAsync<T>(Expression<Func<T, bool>> parameter) where T : class, ITableEntity, new()
        {
            var type = typeof(T);
            var table = await this.GetViewAsync<T>();
            return await table.Where(parameter).FirstOrDefaultAsync();
        }

        public async Task<T> GetAsync<T>(Args<T> parameter = null) where T : class, ITableEntity, new()
        {
            var type = typeof(T);
            var table = await this.GetViewAsync(type);
            string filter = null;
            var attribute = this.GetViewAttribute(type);
            if (parameter != null)
            {
                foreach (var kv in parameter)
                {
                    var propertyName = attribute.PropertyName(type, kv.Key);
                    var condition = TableQuery.GenerateFilterCondition(propertyName, QueryComparisons.Equal, kv.Value);
                    if (filter == null)
                        filter = condition;
                    else
                        filter = TableQuery.CombineFilters(filter, TableOperators.And, condition);
                }
            }

            var query = new TableQuery<T>().Where(filter).Take(1);

            T re = null;
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<T> segment = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                re = segment.Results.FirstOrDefault();
                if (re != null)
                {
                    attribute.AfterLoad(re);
                    return re;
                }
                continuationToken = segment.ContinuationToken;
            } while (continuationToken != null);
            attribute.AfterLoad(re);
            return re;
        }

        public async Task<List<T>> GetAllAsync<T>(Args<T> parameter = null) where T : class, ITableEntity, new()
        {
            string filter = null;
            var type = typeof(T);
            var table = await this.GetViewAsync(type);
            var attribute = this.GetViewAttribute(type);
            if (parameter != null)
            {
                foreach (var kv in parameter)
                {
                    var propertyName = attribute.PropertyName(type, kv.Key);
                    var condition = TableQuery.GenerateFilterCondition(propertyName, QueryComparisons.Equal, kv.Value);
                    if (filter == null)
                        filter = condition;
                    else
                        filter = TableQuery.CombineFilters(filter, TableOperators.And, condition);
                }
            }

            var query = new TableQuery<T>();
            if (filter != null)
            {
                query = query.Where(filter);
            }

            var re = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<T> segment = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                re.AddRange(segment.Results.ToList());
                continuationToken = segment.ContinuationToken;
            } while (continuationToken != null);
            re.ForEach(r => attribute.AfterLoad(r));
            return re;
        }

        public Task<List<ITableEntity>> GetAllAsync(Type type, Args parameter = null)
        {
            return (Task<List<ITableEntity>>)this.GetType().GetMethod(nameof(this.GetAllTableEntitesAsync), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(new[] { type }).Invoke(this, new[] { parameter.As(type) });
        }
        public Task<ITableEntity> GetAsync(Type type, Args parameter = null)
        {
            return (Task<ITableEntity>)this.GetType().GetMethod(nameof(this.GetEntityAsync), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(new[] { type }).Invoke(this, new[] { parameter.As(type) });
        }

        private async Task<ITableEntity> GetEntityAsync<T>(Args<T> parameter = null) where T : class, ITableEntity, new()
        {
            var type = typeof(T);
            var table = await this.GetViewAsync(type);
            string filter = null;
            var attribute = this.GetViewAttribute(type);
            if (parameter != null)
            {
                foreach (var kv in parameter)
                {
                    var propertyName = attribute.PropertyName(type, kv.Key);
                    var condition = TableQuery.GenerateFilterCondition(propertyName, QueryComparisons.Equal, kv.Value);
                    if (filter == null)
                        filter = condition;
                    else
                        filter = TableQuery.CombineFilters(filter, TableOperators.And, condition);
                }
            }

            var query = new TableQuery<T>().Where(filter).Take(1);
            T re = null;
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<T> segment = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                re = segment.Results.FirstOrDefault();
                if (re != null)
                {
                    attribute.AfterLoad(re);
                    return re;
                }
                continuationToken = segment.ContinuationToken;
            } while (continuationToken != null);
            attribute.AfterLoad(re);
            return re;
        }

        private async Task<List<ITableEntity>> GetAllTableEntitesAsync<T>(Args<T> parameter = null) where T : class, ITableEntity, new()
        {
            string filter = null;
            var type = typeof(T);
            var table = await this.GetViewAsync(type);
            var attribute = this.GetViewAttribute(type);
            if (parameter != null)
            {
                foreach (var kv in parameter)
                {
                    var propertyName = attribute.PropertyName(type, kv.Key);
                    var condition = TableQuery.GenerateFilterCondition(propertyName, QueryComparisons.Equal, kv.Value);
                    if (filter == null)
                        filter = condition;
                    else
                        filter = TableQuery.CombineFilters(filter, TableOperators.And, condition);
                }
            }

            var query = new TableQuery<T>();
            if (filter != null)
            {
                query = query.Where(filter);
            }

            var re = new List<ITableEntity>();
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<T> segment = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                re.AddRange(segment.Results.ToList());
                continuationToken = segment.ContinuationToken;
            } while (continuationToken != null);
            return re.Select(r => attribute.AfterLoad(r)).ToList();
        }
    }
}