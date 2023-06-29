using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using TenantManagement.Common.Exceptions;
using TenantManagement.Data.Interfaces;
using TenantManagement.Services.Interfaces;

namespace WebApp.Services
{
    public class ProfileStorageService : IProfileStorageService
    {
        private readonly ILogger<ProfileStorageService> _logger;
        private readonly IMapper _mapper;
        private readonly IProfileStorageRepository _profileStorageRepository;

        public ProfileStorageService(IProfileStorageRepository profileStorageRepository, IMapper mapper, ILogger<ProfileStorageService> logger)
        {
            _profileStorageRepository = profileStorageRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<string> UploadImage(IFormFile image, int id)
        {
            return await _profileStorageRepository.UploadBlob(image, id);
        }

        public async Task DeleteImage(int id)
        {
            await _profileStorageRepository.Delete(id);
        }

        public FileStreamResult GetImage(int id, string context)
        {
            var name = HttpUtility.ParseQueryString(new Uri(context).Query).Get("name");
            Stream file = _profileStorageRepository.GetFile(id, name, out string contentType);
            if (file == null)
            {
                throw new BaseException(System.Net.HttpStatusCode.NotFound, "Image not found", null, _logger);
            }
            return new FileStreamResult(file, contentType)
            {
                FileDownloadName = name
            };
        }
    }
}