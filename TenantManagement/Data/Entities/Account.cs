using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenantManagement.Data.Entities
{
    public class Account : BaseEntity
    {
        public int AccountId { get; set; }
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }

        [MaxLength(1024)]
        public string Name { get; set; }

        [MaxLength(1024)]
        public string ContactFirstName { get; set; }

        [MaxLength(1024)]
        public string ContactLastName { get; set; }

        [MaxLength(1024)]
        public string ContactEmail { get; set; }

        [MaxLength(1024)]
        public string ContactPhone { get; set; }

        [MaxLength(1024)]
        public string TimezoneInfo { get; set; }

        public List<AccountUser> Users { get; set; } = new();
        public List<Address> Addresses { get; set; } = new();

        [NotMapped]
        public Dictionary<string, object> Attributes { get; set; } = new();

        public bool? Enabled { get; set; }
        public string ProfileImageId { get; set; }
    }
}