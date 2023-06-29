namespace TenantManagement.Models
{
    public class UserBaseModel
    {
        public bool? Enabled { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TimezoneInfo { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public bool? Verified { get; set; }
        public string ProfileImageId { get; set; }
    }
}