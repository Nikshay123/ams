using System.ComponentModel.DataAnnotations;

namespace TenantManagement.Data.Entities
{
    public abstract class CacheBaseEntity : BaseEntity
    {
        public readonly string CachePropertyName = "EntityCache";

        [MaxLength]
        public string EntityCache { get; set; }
    }
}