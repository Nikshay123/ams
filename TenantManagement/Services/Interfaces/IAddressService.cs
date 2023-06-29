using System.Collections.Generic;
using System.Threading.Tasks;
using TenantManagement.Data.Entities;

namespace TenantManagement.Services.Interfaces
{
    public interface IAddressService
    {
        Task Add(Address address);

        Task AddRange(List<Address> addresses);

        Task Delete(Address Address);

        Task DeleteRange(List<Address> addresses);

        Task Update(Address address);

        Task UpdateRange(List<Address> addresses);
    }
}