using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenantManagement.Data.Entities;
using TenantManagement.Models;

namespace TenantManagement.Services.Interfaces
{
    public interface IEmailService
    {
        List<EmailModel> Emails { get; }

        EmailModel QueueEmailSpec(string template = null, User from = null, User to = null, Account account = null);

        Task DispatchEmail(Guid? tenantId = null);

        Task<string> SendMail(EmailModel emailSpec);
    }
}