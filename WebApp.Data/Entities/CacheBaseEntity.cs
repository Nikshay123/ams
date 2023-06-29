using System.ComponentModel.DataAnnotations;
using TenantManagement.Data.Entities;

namespace WebApp.Data.Entities
{
    public abstract class CacheBaseEntity : BaseEntity
    {
        public readonly string CachePropertyName = "EntityCache";

        [MaxLength]
        public string EntityCache { get; set; }
    }
}