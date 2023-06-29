using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using TenantManagement.Common.Exceptions;
using TenantManagement.Common.Interfaces;
using TenantManagement.Data.Interfaces;

namespace WebApp.Data.Repositories
{
    public class ProfileStorageRepository : IProfileStorageRepository
    {
        #region Private Read only properties

        private static Mutex _msync = new Mutex(false);

        private readonly string AZURE_STORAGE_CONNECTION_STRING_CONFIG = "AzureStorage";
        private readonly IConfiguration _configuration;
        private readonly string _azureStorageConnectionString;
        private readonly ILogger _logger;
        private readonly IRequestContext _reqContext;
        private BlobContainerClient _blobContainer;

        protected BlobContainerClient BlobContainerClient
        {
            get
            {
                if (_blobContainer == null)
                {
                    try
                    {
                        _msync.WaitOne();
                        if (_blobContainer == null)
                        {
                            var blobService = new BlobServiceClient(_azureStorageConnectionString);
                            _blobContainer = blobService.GetBlobContainerClient(_reqContext.TenantId.ToString());
                            _blobContainer.CreateIfNotExists();
                        }
                    }
                    finally
                    {
                        _msync.ReleaseMutex();
                    }
                }

                return _blobContainer;
            }
        }

        #endregion Private Read only properties

        #region Constructor

        public ProfileStorageRepository(IConfiguration configuration, ILogger<ProfileStorageRepository> logger, IRequestContext requestContext)
        {
            _logger = logger;
            _configuration = configuration;
            _azureStorageConnectionString = _configuration.GetConnectionString(AZURE_STORAGE_CONNECTION_STRING_CONFIG);
            _reqContext = requestContext;
        }

        #endregion Constructor

        #region Public Methods

        public async Task<string> UploadBlob(IFormFile attachment, int principalId)
        {
            string fileName = GetProfileAvatarPath(principalId);
            using (Stream fileStream = new MemoryStream())
            {
                attachment.CopyTo(fileStream);
                fileStream.Position = 0;

                try
                {
                    await BlobContainerClient.DeleteBlobIfExistsAsync(fileName);
                    BlobClient client = BlobContainerClient.GetBlobClient(fileName);
                    await client.UploadAsync(fileStream);
                }
                catch (Exception ex)
                {
                    throw new BaseException(System.Net.HttpStatusCode.BadRequest, $"{nameof(UploadBlob)} Failed: {ex.Message}", ex);
                }
            }

            return $"{fileName}?name={attachment.FileName}&ts={DateTime.UtcNow.Ticks}";
        }

        public Stream GetFile(int principalId, string name, out string contentType)
        {
            contentType = MediaTypeNames.Text.Plain;
            var provider = new FileExtensionContentTypeProvider();

            try
            {
                BlobClient client = BlobContainerClient.GetBlobClient(GetProfileAvatarPath(principalId));

                if (client == null)
                {
                    throw new BaseException(System.Net.HttpStatusCode.NotFound, $"Profile Image Error: Not Found");
                }

                Stream memory = client.OpenRead();
                provider.TryGetContentType(name, out contentType);
                return memory;
            }
            catch (Exception ex)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, $"Profile Image Error: {ex.Message}", ex);
            }
        }

        public async Task Delete(int principalId)
        {
            var fileName = GetProfileAvatarPath(principalId);
            try
            {
                BlobClient client = BlobContainerClient.GetBlobClient(fileName);

                if (client == null)
                {
                    throw new BaseException(System.Net.HttpStatusCode.NotFound, $"Delet Profile Image Error: Not Found");
                }

                await BlobContainerClient.DeleteBlobAsync(fileName);
            }
            catch (Exception ex)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, $"Delete Profile Image Error: {ex.Message}", ex);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private string GetProfileAvatarPath(int principalId)
        {
            return $"azureblob://profile/{_reqContext.ModuleContext}/{principalId}/avatar";
        }

        #endregion Private Methods
    }
}