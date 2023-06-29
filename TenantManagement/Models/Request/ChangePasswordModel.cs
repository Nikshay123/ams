namespace TenantManagement.Models.Request
{
    public class ChangePasswordModel
    {
        public string Previous { get; set; }

        public string New { get; set; }
    }
}