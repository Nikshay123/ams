namespace TenantManagement.Models
{
    public class AccountBaseModel
    {
        public int AccountId { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string Name { get; set; }
        public bool? Enabled { get; set; }
        public string TimezoneInfo { get; set; }
    }
}