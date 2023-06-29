using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace TenantManagement.Data.Interfaces
{
    public interface IProfileStorageRepository
    {
        Task Delete(int principalId);

        Stream GetFile(int principalId, string name, out string contentType);

        Task<string> UploadBlob(IFormFile attachment, int principalId);
    }
}