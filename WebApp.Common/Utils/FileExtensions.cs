using WebApp.Common.Constants;
using WebApp.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Common.Utils
{
    public static class FileExtensions
    {
        #region Public Methods

        public static bool Validate(this List<IFormFile> files)
        {
            if (files == null || files.Count <= 0)
            {
                return false;
            }

            files.CheckLimit();
            files.CheckContentType();
            files.CheckExtension();
            files.CheckSize();

            return true;
        }

        public static bool GetContentType(this FileExtensionContentTypeProvider fileProvider, string fileName, out string contentType)
        {
            fileProvider.TryGetContentType(fileName, out contentType);

            if (contentType == null)
            {
                string extension = GetExtensionType(fileName);

                if (extension == ExtensionTypes.HEIC || extension == ExtensionTypes.HEIF)
                {
                    contentType = ContentTypes.ApplicationHEICnHEIF;
                }
            }

            return true;
        }

        #endregion Public Methods

        #region Private Methods

        private static bool CheckLimit(this List<IFormFile> files)
        {
            if (files.Count > AppConstants.FileLimit)
            {
                throw new ApiException(ErrorResponse.ErrorEnum.FileLimitExceeded);
            }

            return true;
        }

        private static bool CheckContentType(this List<IFormFile> files)
        {
            foreach (var file in files)
            {
                if (file.ContentType != ContentTypes.ApplicationJPEG && file.ContentType != ContentTypes.ApplicationPNG && file.ContentType != ContentTypes.ApplicationPDF && file.ContentType != ContentTypes.ApplicationDOCX && file.ContentType != ContentTypes.ApplicationHEICnHEIF)
                {
                    throw new ApiException(ErrorResponse.ErrorEnum.FileContentType);
                }
            }

            return true;
        }

        private static bool CheckExtension(this List<IFormFile> files)
        {
            foreach (var file in files)
            {
                string extension = file.FileName.Split(".").Last().ToLower();

                if (extension != ExtensionTypes.JPEG && extension != ExtensionTypes.JPG && extension != ExtensionTypes.PNG && extension != ExtensionTypes.PDF && extension != ExtensionTypes.DOCX && extension != ExtensionTypes.HEIC && extension != ExtensionTypes.HEIF)
                {
                    throw new ApiException(ErrorResponse.ErrorEnum.FileExtension);
                }
            }

            return true;
        }

        private static bool CheckSize(this List<IFormFile> files)
        {
            long fileSize = 0;

            foreach (var file in files)
            {
                fileSize += file.Length;
            }

            if (fileSize > AppConstants.FileSize)
            {
                throw new ApiException(ErrorResponse.ErrorEnum.FileSizeExceeded);
            }

            return true;
        }

        private static string GetExtensionType(string fileName)
        {
            var extension = fileName.Split('.').Last();
            return extension.ToLower();
        }

        #endregion Private Methods
    }
}