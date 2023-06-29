using System.Collections.Generic;
using System.Threading.Tasks;
using TenantManagement.Data.Entities;

namespace TenantManagement.Data.Interfaces
{
    public interface IAddressRepository
    {
        Task Add(Address address);
        Task AddRange(List<Address> addresses);
        Task Delete(Address address);
        Task DeleteRange(List<Address> addresses);
        Task Update(Address address);
        Task UpdateRange(List<Address> addresses);
    }
}