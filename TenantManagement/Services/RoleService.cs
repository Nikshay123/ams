using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Entities;
using TenantManagement.Data.Interfaces;
using TenantManagement.Services.Interfaces;

namespace TenantManagement.Services
{
    public class RoleService : IRoleService
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IRoleRepository _rolerepo;
        private readonly IRequestContext _reqContext;
        private readonly ILogger<RoleService> _logger;

        public RoleService(IMapper mapper, IConfiguration configuration, IRoleRepository rolerepo, IRequestContext reqcontext, ILogger<RoleService> logger)
        {
            _mapper = mapper;
            _rolerepo = rolerepo;
            _reqContext = reqcontext;
            _logger = logger;
        }

        public async Task<T> GetRole<T>(int? roleId = null, string name = null)
        {
            if (roleId != null)
            {
                return _mapper.Map<Role, T>(await _rolerepo.GetByIdAsync(roleId.Value));
            }

            return _mapper.Map<Role, T>(await _rolerepo.GetByNameAsync(name));
        }

        public async Task<List<T>> GetRoles<T>(List<int> roleIds = null)
        {
            return _mapper.Map<List<Role>, List<T>>(await _rolerepo.GetRolesAsync(roleIds));
        }

        public async Task<List<T>> GetRoles<T>(List<Roles> roles = null)
        {
            return _mapper.Map<List<Role>, List<T>>(await _rolerepo.GetRolesAsync(roles));
        }

        public async Task<List<T>> GetRoles<T>(List<string> roles = null)
        {
            return _mapper.Map<List<Role>, List<T>>(await _rolerepo.GetRolesAsync(roles.Select(r => (Roles)Enum.Parse(typeof(Roles), r)).ToList()));
        }

        public async Task DeleteRole(Role role)
        {
            await _rolerepo.Delete(role);
        }
    }
}