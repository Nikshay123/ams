using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Data.Interfaces;
using TenantManagement.Services.Interfaces;

namespace TenantManagement.Services
{
    public class AddressService : IAddressService
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IAddressRepository _addressRepo;
        private readonly IRequestContext _reqContext;
        private readonly ILogger<AddressService> _logger;

        public AddressService(IMapper mapper, IConfiguration configuration, IAddressRepository addressrepo, IRequestContext reqcontext, ILogger<AddressService> logger)
        {
            _mapper = mapper;
            _addressRepo = addressrepo;
            _reqContext = reqcontext;
            _logger = logger;
        }

        public async Task Add(Address address)
        {
            await _addressRepo.Add(address);
        }

        public async Task AddRange(List<Address> addresses)
        {
            await _addressRepo.AddRange(addresses);
        }

        public async Task Update(Address address)
        {
            await _addressRepo.Update(address);
        }

        public async Task UpdateRange(List<Address> addresses)
        {
            await _addressRepo.UpdateRange(addresses);
        }

        public async Task Delete(Address Address)
        {
            await _addressRepo.Delete(Address);
        }

        public async Task DeleteRange(List<Address> addresses)
        {
            await _addressRepo.DeleteRange(addresses);
        }
    }
}