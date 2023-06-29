using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenantManagement.Common.Enums;

namespace TenantManagement.Data.Entities
{
    public class Address : BaseEntity
    {
        public int AddressId { get; set; }

        [Column(TypeName = ("nvarchar(1024)"))]
        public AddressType Type { get; set; }

        [MaxLength(1024)]
        public string StreetAddress { get; set; }

        [MaxLength(1024)]
        public string StreetAddress2 { get; set; }

        [MaxLength(1024)]
        public string City { get; set; }

        [MaxLength(1024)]
        public string State { get; set; }

        [MaxLength(1024)]
        public string PostalCode { get; set; }

        [MaxLength(1024)]
        public string Country { get; set; }

        [MaxLength(1024)]
        public string Phone { get; set; }

        public List<Account> Accounts { get; set; } = new();
        public List<User> Users { get; set; } = new();
    }
}