using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.WindowsAzure.Storage.Table;
using Vabulu.Attributes;
using Vabulu.Tables;

namespace Vabulu.Models {
    public class User : IdentityUser {
        public string Language { get; set; }

        public static implicit operator User(UserEntity user) => user == null ? null : new User {
            UserName = user.UserName,
            NormalizedUserName = user.NormalizedUserName,
            Id = user.UserId,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            NormalizedEmail = user.NormalizedEmail,
            PasswordHash = user.PasswordHash,
            Language = user.Language
        };
    }
}