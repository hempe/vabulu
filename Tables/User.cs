using System.Linq;
using Microsoft.AspNetCore.Identity;
using Vabulu.Attributes;
using Vabulu.Models;

namespace Vabulu.Tables {
    [Table("Users")]
    public class UserEntity : UserData {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public string NormalizedUserName { get; set; }

        [SecretValue]
        public string PasswordHash { get; set; }
        public bool EmailConfirmed { get; set; }
        public UserEntity() { }

        public static implicit operator UserEntity(User user) => user == null ? null : new UserEntity {
            UserName = user.UserName,
            NormalizedUserName = user.NormalizedUserName,
            UserId = user.Id,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            NormalizedEmail = user.NormalizedEmail,
            PasswordHash = user.PasswordHash
        };
    }
}