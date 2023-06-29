using Azure.Storage.Blobs;
using WebApp.Common.Constants;
using WebApp.Common.Exceptions;
using WebApp.Data.Entities;
using WebApp.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenantManagement.Common;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Interfaces;
using Azure;
using System.Net;

namespace WebApp.Data.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        #region Private Read only properties

        private readonly string AZURE_STORAGE_CONNECTION_STRING_CONFIG = "AzureStorage";
        private readonly WebAppContext _context;
        private readonly BlobServiceClient blobService;
        private readonly BlobContainerClient blobContainer;
        private readonly IConfiguration _configuration;
        private readonly string _azureStorageConnectionString;
        private readonly ILogger _logger;
        private readonly IRequestContext _requestContext;
        private readonly ITenantDbContextFactory _tenantDbContextFactory;

        #endregion Private Read only properties

        #region Constructor

        public AttachmentRepository(ITenantDbContextFactory tenantDbContextFactory, IConfiguration configuration, ILogger<AttachmentRepository> logger, IRequestContext requestContext)
        {
            _tenantDbContextFactory = tenantDbContextFactory;
            _context = _tenantDbContextFactory.DbContext<WebAppContext>();
            _logger = logger;
            _configuration = configuration;
            _azureStorageConnectionString = _configuration.GetConnectionString(AZURE_STORAGE_CONNECTION_STRING_CONFIG);
            blobService = new BlobServiceClient(_azureStorageConnectionString);
            blobContainer = blobService.GetBlobContainerClient(requestContext.TenantId.ToString());
            blobContainer.CreateIfNotExists();
            _requestContext = requestContext;
        }

        #endregion Constructor

        #region Public Methods

        public async Task<BaseAttachment> UploadBlob(IFormFile file, BaseAttachment attachment)
        {
            IQueryable<BaseAttachment> query = _context.Attachments;

            using (Stream fileStream = new MemoryStream())
            {
                string fileName = $" {attachment.GetType().Name}/{attachment.PrincipalId}/{file.FileName}";
                attachment.Name = file.FileName;
                attachment.Location = fileName;
                attachment.AccessToken = GetHashValueByString(fileName);
                file.CopyTo(fileStream);
                fileStream.Position = 0;

                try
                {
                    var fileObject = await GetFileObject(query, attachment.PrincipalId).Select(attach => attach).ToListAsync();

                    if (fileObject != null)
                    {
                        var nameList = fileObject.Select(attach => attach.Name).ToList();

                        if (nameList.Contains(file.FileName))
                        {
                            BaseAttachment delAttachment = fileObject.Where(attach => attach.Name == file.FileName).FirstOrDefault();
                            _context.Attachments.Remove(delAttachment);
                            await _context.SaveChangesAsync();
                        }
                    }

                    await blobContainer.DeleteBlobIfExistsAsync(fileName);
                    BlobClient client = blobContainer.GetBlobClient(fileName);
                    await client.UploadAsync(fileStream);
                    await _context.Attachments.AddAsync(attachment);
                    await _context.SaveChangesAsync();
                    return attachment;
                }
                catch (Exception)
                {
                    throw new ApiException(ErrorResponse.ErrorEnum.UploadFileError);
                }
            }
        }

        public async Task<List<BaseAttachment>> ListOfFiles(int principalId)
        {
            IQueryable<BaseAttachment> query = _context.Attachments;
            var fileObject = await GetFileObject(query, principalId).ToListAsync();
            if (fileObject == null)
            {
                return null;
            }
            return fileObject;
        }

        public Stream GetFile(int principalId, int attachmentId, string accessToken, out string fileName, out string contentType)
        {
            var context = _tenantDbContextFactory.DbContext<WebAppContext>();
            contentType = ContentTypes.TextPlain;
            fileName = string.Empty;
            IQueryable<BaseAttachment> query = context.Attachments;

            var fileObject = GetFileObject(query, principalId, attachmentId).FirstOrDefault();

            if (fileObject == null || fileObject.Location == null)
            {
                throw new ApiException(ErrorResponse.ErrorEnum.NotFound);
            }

            if (fileObject.AccessToken != accessToken)
            {
                throw new ApiException(ErrorResponse.ErrorEnum.UnauthorizedError);
            }

            fileName = fileObject.Name;
            var provider = new FileExtensionContentTypeProvider();

            try
            {
                BlobClient client = blobContainer.GetBlobClient(fileObject.Location);

                if (client == null)
                {
                    throw new ApiException(ErrorResponse.ErrorEnum.NotFound);
                }

                Stream memory = client.OpenRead();
                provider.TryGetContentType(fileName, out contentType);
                return memory;
            }
            catch (Exception)
            {
                throw new ApiException(ErrorResponse.ErrorEnum.ContentTypeError);
            }
        }

        public async Task<bool> Delete(int principal, int attachmentId)
        {
            IQueryable<BaseAttachment> query = _context.Attachments;

            var fileObject = await GetFileObject(query, principal, attachmentId).FirstOrDefaultAsync();
            try
            {
                if (fileObject != null && fileObject.Location != null)
                {
                    BlobClient client = blobContainer.GetBlobClient(fileObject.Location);

                    if (client == null)
                    {
                        throw new ApiException(ErrorResponse.ErrorEnum.NotFound);
                    }

                    try
                    {
                        await blobContainer.DeleteBlobAsync(fileObject.Location);
                    }
                    catch (RequestFailedException e)
                    {
                        if (e.Status != (int)HttpStatusCode.NotFound)
                        {
                            return false;
                        }
                    }

                    _context.Attachments.Remove(fileObject);
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public async Task<bool> DeleteMultiple(int principalId, List<int> attachmentIds)
        {
            if (attachmentIds == null)
            {
                var files = await ListOfFiles(principalId);
                if (files != null)
                {
                    attachmentIds = files.Select(a => a.AttachmentId).ToList();
                }
            }

            if (attachmentIds != null && attachmentIds.Count > 0)
            {
                foreach (var attachmentId in attachmentIds)
                {
                    bool isDeleted = await Delete(principalId, attachmentId);
                    if (!isDeleted)
                    {
                        continue;
                    }
                }
            }
            return true;
        }

        #endregion Public Methods

        #region Private Methods

        private static IQueryable<BaseAttachment> GetFileObject(IQueryable<BaseAttachment> query, int owner, int? attachmentId = null)
        {
            try
            {
                if (attachmentId == null)
                {
                    var fileObject = query.Where(attach => attach.PrincipalId == owner).Select(attach => attach);
                    return fileObject;
                }
                else
                {
                    var fileObject = query.Where(attach => attach.PrincipalId == owner & attach.AttachmentId == attachmentId).Select(attach => attach);
                    return fileObject;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetHashValueByString(string fileName)
        {
            string path = CryptoUtils.GetRandomString();

            if (String.IsNullOrEmpty(fileName) || String.IsNullOrEmpty(path))
            {
                return null;
            }

            using (var sha = System.Security.Cryptography.SHA1.Create())
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(fileName + path);
                byte[] hashbytes = sha.ComputeHash(byteArray);

                string hash = _requestContext.TenantId + "." + BitConverter.ToString(hashbytes).Replace("-", string.Empty);
                return hash;
            }
        }

        #endregion Private Methods
    }
}