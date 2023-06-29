using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TenantManagement.Services.Interfaces
{
    public interface IProfileStorageService
    {
        Task<string> UploadImage(IFormFile image, int id);
        Task DeleteImage(int id);
        FileStreamResult GetImage(int id, string name);
    }
}