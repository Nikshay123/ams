using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenantManagement.Data.Entities
{
    public class User : BaseEntity
    {
        public int UserId { get; set; }
        public Tenant Tenant { get; set; }
        public Guid TenantId { get; set; }

        [MaxLength(1024)]
        public string FirstName { get; set; }

        [MaxLength(1024)]
        public string LastName { get; set; }

        [MaxLength(1024)]
        public string Username { get; set; }

        [MaxLength(1024)]
        public string ContactPhone { get; set; }

        [MaxLength(1024)]
        public string Userhash { get; set; }

        public List<Role> Roles { get; set; } = new();
        public List<Scope> Scopes { get; set; } = new();

        //This will typically be auto filtered to only have enabled accounts
        public List<AccountUser> Accounts { get; set; } = new();

        //This is the original fetched accounts as part of any include
        [NotMapped]
        public List<AccountUser> TrackedDisabledAccounts { get; set; } = new();

        public List<Address> Addresses { get; set; } = new();

        [MaxLength(1024)]
        public string Salt { get; set; }

        [MaxLength(1024)]
        public string RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiry { get; set; }

        [MaxLength(1024)]
        public string TransientAuthToken { get; set; }

        public DateTime? TransientAuthExpiry { get; set; }
        public string TransientContext { get; set; }
        public bool? Verified { get; set; }
        public bool? Enabled { get; set; }
        public DateTime? LatestLogin { get; set; }

        [MaxLength(1024)]
        public string TimezoneInfo { get; set; }

        public string ProfileImageId { get; set; }
    }
}