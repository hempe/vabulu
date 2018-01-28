using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.WindowsAzure.Storage.Table;
using Vabulu.Attributes;

namespace Vabulu.Tables {

    [Table("Properties")]
    public class Property : TableEntity {

        [IgnoreProperty]
        [RowKey]
        public string Id { get; set; }

        public Property() {
            this.PartitionKey = "0";
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public static implicit operator Property(Models.Property x) => x == null ? null : new Property {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description
        };

        public static implicit operator Models.Property(Property x) => x == null ? null : new Models.Property {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description
        };
    }
}