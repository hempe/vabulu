using Microsoft.AspNetCore.Identity;
using Microsoft.WindowsAzure.Storage.Table;
using Vabulu.Attributes;

namespace Vabulu.Tables {

    [Table("UserRoles")]
    public class UserRoleEntity : UserData {

        [IgnoreProperty]
        [RowKey]
        public string RoleName { get; set; }
    }

    [Table("Roles")]
    public class RoleEntity : TableEntity {

        [IgnoreProperty]
        [PartitionKey]
        public string Id { get; set; }

        public string ConcurrencyStamp { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }

        public RoleEntity() {
            this.RowKey = "0";
        }

        public static implicit operator RoleEntity(IdentityRole role) => role == null ? null : new RoleEntity {
            ConcurrencyStamp = role.ConcurrencyStamp,
            Id = role.Id,
            Name = role.Name,
            NormalizedName = role.NormalizedName
        };

        public static implicit operator IdentityRole(RoleEntity role) => role == null ? null : new IdentityRole {
            ConcurrencyStamp = role.ConcurrencyStamp,
            Id = role.Id,
            Name = role.Name,
            NormalizedName = role.NormalizedName
        };
    }
}