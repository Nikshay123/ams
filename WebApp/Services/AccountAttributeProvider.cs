using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using WebApp.Data.Entities;

namespace WebApp.Services
{
    //Support for adding business related data to an account in a generic fashion
    public class AccountAttributeProvider : IAccountAttributeProvider
    {
        private IMapper _mapper;
        private ILogger<AccountAttributeProvider> _logger;

        public AccountAttributeProvider(IMapper mapper, ILogger<AccountAttributeProvider> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public async Task GetAttributes(IRequestContext requestCtx, Account account, string include)
        {
            account.Attributes.Add(nameof(Sample), "Sample Account Attribute");
        }
    }
}