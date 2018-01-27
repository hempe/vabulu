using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using Vabulu.Attributes;
using Vabulu.Middleware;
using Vabulu.Models;
using Vabulu.Services;
using Vabulu.Services.I18n;
using Vabulu.Tables;
using Vabulu.Views;

namespace Vabulu.Controllers {

    public class TableEntry {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Table { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }

    [Route("api/admin")]
    [Authorize(Roles = "admin")]
    [Authorize]
    public class AdminController : BaseController {
        public AdminController(UserManager<User> userManager, TableStore tableStore) : base(userManager, tableStore) { }

        [HttpGet("tables")]
        [ProducesResponseType(typeof(string[]), 200)]
        public IActionResult GetTables() {
            var tables = this.GetType().Assembly.GetTypes()
                .Where(t => t.IsTable())
                .Select(t => t.Table().TableName).ToList();
            return this.Ok(tables);
        }

        [HttpGet("tables/{tableName}")]
        [ProducesResponseType(typeof(TableEntity[]), 200)]
        public async Task<IActionResult> GetAll([FromRoute] string tableName) {
            var table = this.FindTableType(tableName);
            var users = await this.GetUserMappingAsync();
            var all = await this.TableStore.GetAllAsync(table, new Args { });
            var cleaned = this.ToTableEntries(table, all, false, users);
            return this.Ok(cleaned);
        }

        [HttpGet("tables/headers/{tableName}")]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        public IActionResult TableHeaders([FromRoute] string tableName) {
            var table = this.FindTableType(tableName);
            var defs = this.GetTableProperties(table, false)
                .ToDictionary(p => p.Name, p =>(object) p.PropertyType.IsSimple());

            if (defs.ContainsKey(nameof(UserEntity.UserId))) {
                defs.Remove(nameof(UserEntity.UserId));
                defs[nameof(UserEntity.UserName)] = true;
            }

            return this.Ok(defs);
        }

        [HttpGet("tables/headers/detailed/{tableName}")]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        public IActionResult EntryHeader([FromRoute] string tableName) {
            var table = this.FindTableType(tableName);
            var defs = this.GetTableProperties(table, true)
                .ToDictionary(p => p.Name, p =>(object) (p.PropertyType.IsSimple() ? "text" : "object"));

            if (defs.ContainsKey(nameof(UserEntity.UserId)))
                defs[nameof(UserEntity.UserId)] = "readonly";

            if (defs.ContainsKey(nameof(UserEntity.UserId)) && !defs.ContainsKey(nameof(UserEntity.UserName)))
                defs[nameof(UserEntity.UserName)] = "readonly";

            return this.Ok(defs);
        }

        [HttpGet("tables/{tableName}/{partitionKey}/{rowKey}")]
        [ProducesResponseType(typeof(TableEntity), 200)]
        public async Task<IActionResult> GetByPartitionAndRowKey([FromRoute] string tableName, [FromRoute] string partitionKey, [FromRoute] string rowKey) {
            var table = this.FindTableType(tableName);
            var users = await this.GetUserMappingAsync();

            var value = await this.TableStore.GetAsync(table,
                new Args { { nameof(ITableEntity.PartitionKey), partitionKey }, { nameof(ITableEntity.RowKey), rowKey }
                });
            return this.Ok(this.ToTableEntry(table, value, true, users));
        }

        [HttpDelete("tables/{tableName}/{partitionKey}/{rowKey}")]
        [ProducesResponseType(typeof(TableEntity), 200)]
        public async Task<IActionResult> Delete([FromRoute] string tableName, [FromRoute] string partitionKey, [FromRoute] string rowKey) {
            var table = this.FindTableType(tableName);
            if (table == typeof(UserEntity)) {
                var user = await this.UserManager.FindByIdAsync(partitionKey);
                if (user == null)
                    return this.BadRequest();

                await this.UserManager.DeleteAsync(user);
                return this.NoContent();
            }

            var entry = await this.TableStore.GetAsync(table,
                new Args { { nameof(ITableEntity.PartitionKey), partitionKey }, { nameof(ITableEntity.RowKey), rowKey }
                });
            if (entry == null)
                return this.BadRequest();
            await this.TableStore.DeleteAsync(table, entry);

            return this.NoContent();
        }

        [HttpPost("tables/{tableName}/{partitionKey}/{rowKey}")]
        [ProducesResponseType(typeof(TableEntity), 200)]
        public async Task<IActionResult> Update([FromRoute] string tableName, [FromRoute] string partitionKey, [FromRoute] string rowKey, [FromBody] TableEntry tableEntry) {
            var table = this.FindTableType(tableName);
            var users = await this.GetUserMappingAsync();

            var entity = await this.TableStore.GetAsync(table,
                new Args { { nameof(ITableEntity.PartitionKey), partitionKey }, { nameof(ITableEntity.RowKey), rowKey }
                });

            if (entity == null)
                return BadRequest();
            entity = this.UpdateTableEntry(table, entity, tableEntry, true);
            await this.TableStore.AddOrUpdateAsync(table, entity);
            entity = await this.TableStore.GetAsync(table,
                new Args { { nameof(ITableEntity.PartitionKey), partitionKey }, { nameof(ITableEntity.RowKey), rowKey }
                });

            return this.Ok(this.ToTableEntry(table, entity, true, users));
        }

        private List<System.Reflection.PropertyInfo> GetTableProperties(Type type, bool includeObjects) {
            var toIgnore = type.GetProperties()
                .Where(p => p.HasCustomAttribute<JsonDataAttribute>())
                .Select(p => p.GetCustomAttribute<JsonDataAttribute>().PropertyName)
                .ToList();

            var query = type.GetProperties()
                .Where(p => !p.HasCustomAttribute<SecretValueAttribute>())
                .Where(p => !toIgnore.Contains(p.Name))
                .Where(p => typeof(ITableEntity).GetProperty(p.Name) == null);

            if (includeObjects)
                return query.ToList();

            return query
                .Where(p => !p.Name.StartsWith("normalized", StringComparison.InvariantCultureIgnoreCase))
                .Where(p => p.PropertyType.IsSimple())
                .ToList();
        }

        private ITableEntity UpdateTableEntry(Type type, ITableEntity entity, TableEntry entry, bool includeObjects) {
            var tableName = type.Table().TableName;
            var properties = this.GetTableProperties(type, includeObjects);

            foreach (var property in properties) {
                if (entry.Data.TryGetValue(property.Name, out var v, StringComparison.InvariantCultureIgnoreCase)) {
                    var value = v;
                    if (value is JObject jobj) {
                        value = jobj.ToObject(property.PropertyType);
                    } else {
                        value = Convert.ChangeType(value, property.PropertyType, CultureInfo.InvariantCulture);
                    }
                    property.SetValue(entity, value);
                }
            }
            return entity;
        }

        private TableEntry ToTableEntry(Type type, ITableEntity value, bool includeObjects, Dictionary<string, string> userMappings) {
            if (value == null)
                return null;
            var tableName = type.Table().TableName;
            var properties = this.GetTableProperties(type, includeObjects);

            var v = new TableEntry {
                PartitionKey = value.PartitionKey,
                RowKey = value.RowKey,
                Table = tableName,
                Data = new Dictionary<string, object>()
            };

            foreach (var property in properties) {
                v.Data[property.Name] = property.GetValue(value);
                if (property.Name == nameof(UserData.UserId)) {
                    v.Data[nameof(UserEntity.UserName)] = userMappings.GetValueOrDefault((string) v.Data[property.Name]) ?? "UNKNONW";
                }
            }
            return v;
        }

        private async Task<Dictionary<string, string>> GetUserMappingAsync() {
            var all = await this.TableStore.GetAllAsync<UserLookupEntity>(new Args { });
            return all.ToDictionary(x => x.UserId, x => x.UserName);
        }

        private List<TableEntry> ToTableEntries(Type type, List<ITableEntity> values, bool includeObjects, Dictionary<string, string> userMappings) {
            var rows = new List<TableEntry>();
            if (values == null)
                return rows;
            var tableName = type.Table().TableName;
            var properties = this.GetTableProperties(type, includeObjects);
            foreach (var obj in values) {
                if (obj == null)
                    continue;
                var value = new TableEntry {
                    PartitionKey = obj.PartitionKey,
                    RowKey = obj.RowKey,
                    Table = tableName,
                    Data = new Dictionary<string, object>()
                };

                foreach (var property in properties) {
                    value.Data[property.Name] = property.GetValue(obj);
                    if (property.Name == nameof(UserData.UserId)) {
                        value.Data[nameof(UserEntity.UserName)] = userMappings.GetValueOrDefault((string) value.Data[property.Name]) ?? "UNKNONW";
                    }
                }
                rows.Add(value);
            }

            return rows;
        }

        private Type FindTableType(string tableName) {
            var type = this.GetType().Assembly.GetTypes()
                .Where(t => t.IsTable())
                .Where(t => string.Equals(t.Table().TableName, tableName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
            return type;
        }
    }
}