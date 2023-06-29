using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenantManagement.Data.Entities
{
    public class Tenant : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid TenantId { get; set; }
        [MaxLength(1024)]
        public string Name { get; set; }
        [MaxLength(1024)]
        public string ContactName { get; set; }
        [MaxLength(1024)]
        public string ContactEmail { get; set; }
        [MaxLength(1024)]
        public string ContactPhone { get; set; }
        [MaxLength(1024)]
        public string TimezoneInfo { get; set; }
        public List<User> Users { get; set; } = new();
        public List<Account> Accounts { get; set; } = new();
    }
}