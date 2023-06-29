using System.Collections.Generic;
using System.Text.Json.Serialization;
using TenantManagement.Common;
using TenantManagement.Data.Entities;

namespace TenantManagement.Models
{
    public abstract class NotificationAbstract
    {
        public string MsgBody { get; set; }
        public string Subject { get; set; }
    }

    public class EmailModel : NotificationAbstract
    {
        public string Recipients { get; set; }

        [JsonIgnore]
        public string Template { get; set; }

        [JsonIgnore]
        public Dictionary<string, object> ModelData { get; set; } = new();

        public EmailModel AddFrom(User from)
        {
            ModelData[nameof(EmailDataKeys.FromName)] = $"{from.FirstName} {from.LastName}";
            ModelData[nameof(EmailDataKeys.FromEmail)] = from.Username;
            return this;
        }

        public EmailModel AddTo(User to)
        {
            Recipients ??= to.Username;
            ModelData[nameof(EmailDataKeys.Name)] = to.FirstName;
            return this;
        }

        public EmailModel AddAccount(Account account)
        {
            ModelData[nameof(EmailDataKeys.AccountName)] = account.Name;
            return this;
        }
    }

    public class SMSModel : NotificationAbstract
    {
        public string SMSNumber { get; set; }
    }
}