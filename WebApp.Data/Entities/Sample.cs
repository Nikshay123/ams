using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenantManagement.Data.Entities;

namespace WebApp.Data.Entities
{
    public enum SampleEnum
    {
        EnumVal1,
        EnumVal2
    }

    public class Sample : BaseEntity
    {
        [Key]
        public int SampleId { get; set; }

        [MaxLength(1024)]
        public string StringValue { get; set; }

        public int IntValue { get; set; }

        [Column(TypeName = ("decimal(15,5)"))]
        public decimal DecimalValue { get; set; }

        [Column(TypeName = ("nvarchar(50)"))]
        public SampleEnum EnumStoredAsString { get; set; }
    }
}