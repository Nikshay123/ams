using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenantManagement.Data.Entities;

namespace WebApp.Data.Entities
{
    public class BaseAttachment : BaseEntity
    {
        [Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AttachmentId { get; set; }

        //NOTE: This is a shortcut to the Attachment Owner Id and must be overridden in derived classes
        [NotMapped]
        public virtual int PrincipalId
        {
            get { return 0; /*NOTE: Derived Class Get Main Attachment Type Id */ }
            set { /*NOTE: Derived Class Set Main Attachment Type Id */ }
        }

        [NotMapped]
        public IFormFile Attachment { get; set; }

        //Table Inheritance Col
        public string Discriminator { get; set; }

        [Column(TypeName = "nvarchar(1024)")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar(1024)")]
        public string Location { get; set; }

        [Column(TypeName = "nvarchar(1024)")]
        public string AccessToken { get; set; }
    }
}