using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TenantManagement.Data.Entities
{
    public class Scope : BaseEntity
    {
        public int ScopeId { get; set; }

        [MaxLength(1024)]
        public string Name { get; set; }

        public List<User> Users { get; set; } = new();
    }
}