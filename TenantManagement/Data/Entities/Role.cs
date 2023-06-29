using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TenantManagement.Common;

namespace TenantManagement.Data.Entities
{
    public class Role : BaseEntity
    {
        public int RoleId { get; set; }

        [Column(TypeName = ("nvarchar(1024)"))]
        public Roles Name { get; set; }

        public List<User> Users { get; set; } = new();

        public List<AccountUser> AccountUsers { get; set; } = new();
    }
}