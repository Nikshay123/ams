using WebApp.Data.Entities;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WebApp.Data.Repositories.Interfaces
{
    public interface IAttachmentRepository
    {
        Task<BaseAttachment> UploadBlob(IFormFile file, BaseAttachment attachment);

        Task<List<BaseAttachment>> ListOfFiles(int principalId);

        Stream GetFile(int principalId, int attachmentId, string accessToken, out string fileName, out string contentType);

        Task<bool> Delete(int principalId, int attachmentId);

        Task<bool> DeleteMultiple(int principalId, List<int> attachmentIds);
    }
}