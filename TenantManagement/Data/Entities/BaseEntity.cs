using System;
using System.ComponentModel.DataAnnotations;

namespace TenantManagement.Data.Entities
{
    public abstract class BaseEntity : ICloneable
    {
        public DateTime CreatedDatetime { get; set; }
        public DateTime? ModifiedDatetime { get; set; }

        [MaxLength(1024)]
        public string CreatedBy { get; set; }

        [MaxLength(1024)]
        public string ModifiedBy { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}