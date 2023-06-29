using WebApp.Data.Entities;

namespace WebApp.Models
{
    public class SampleEntityModel
    {
        public int? SampleEntityId { get; set; }

        public string StringValue { get; set; }

        public int? IntValue { get; set; }

        public decimal? DecimalValue { get; set; }

        public SampleEnum? EnumStoredAsString { get; set; }
    }
}