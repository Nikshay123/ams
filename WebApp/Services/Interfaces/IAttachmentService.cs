using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebApp.Data.Entities;
using WebApp.Models;

namespace WebApp.Services.Interfaces
{
    public interface IAttachmentService
    {
        Task<List<T>> UploadFiles<T>(List<BaseAttachment> attachments);

        Task<List<T>> ListOfFiles<T>(int principalId);

        Stream DownloadFile(int principalId, int attachmentId, string accessToken, out string fileName, out string contentType);

        Task DeleteBlob(int principalId, int attachmentId);

        Task DeleteRange(int principalId, List<int> attachmentIds);
    }
}