using System.Collections.Generic;

namespace WebApp.Common.Exceptions
{
    public static class ErrorResponse
    {
        private const int MAX_FILE_MB_LIMIT = 10;
        private const int MAX_FILES_LIMIT = 10;

        public enum ErrorEnum
        {
            NullObject = 700,
            Validation = 701,
            Duplicate = 702,
            FileSizeExceeded = 703,
            FileContentType = 704,
            FileExtension = 705,
            FileLimitExceeded = 706,
            NotFound = 404,
            UploadFileError = 500,
            BadRequest = 400,
            ConflictError = 409,
            ContentTypeError = 415,
            DatabaseError = 422,
            UnauthorizedError = 401
        };

        private static readonly Dictionary<ErrorEnum, string> ErrorDict = new Dictionary<ErrorEnum, string>
        {
            {
                ErrorEnum.NullObject, "Model cannot be null"
            },
            {
                ErrorEnum.Validation, "Validation failed for the payload"
            },
            {
                ErrorEnum.Duplicate, "Duplicate Entry is not allowed"
            },
            {
                ErrorEnum.FileSizeExceeded, $"File size can not be greater than {MAX_FILE_MB_LIMIT} MB"
            },
            {
                ErrorEnum.FileExtension, "File extensions are not proper"
            },
            {
                ErrorEnum.FileContentType, "File content type are not proper"
            },
            {
                ErrorEnum.FileLimitExceeded, $"Only {MAX_FILES_LIMIT} files are allowed"
            },
            {
                ErrorEnum.NotFound, "Records not found"
            },
            {
                ErrorEnum.UploadFileError, "Error occurred while uploading the file"
            },
            {
                ErrorEnum.BadRequest, "Error occurred while processing request"
            },
            {
                ErrorEnum.ConflictError, "Unable to process request due to a data conflict"
            },
            {
                ErrorEnum.ContentTypeError, "An error occurred while getting file"
            },
            {
                ErrorEnum.DatabaseError, "A data processing error occurred"
            },
            {
                ErrorEnum.UnauthorizedError, "Unauthorized Access"
            }
        };

        public static string GetErrorMessage(ErrorEnum errorCode)
        {
            return ErrorDict[errorCode];
        }

        private static string GetErrorCode(ErrorEnum errorCode)
        {
            int code = (int)errorCode;
            return code.ToString();
        }

        public static string ToErrorCodeString(this ErrorEnum errorCode)
        {
            return GetErrorCode(errorCode);
        }
    }
}