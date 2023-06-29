using AutoMapper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Common.Exceptions;
using WebApp.Common.Utils;
using WebApp.Data.Entities;
using WebApp.Data.Repositories.Interfaces;
using WebApp.Services.Interfaces;

namespace WebApp.Services
{
    public class AttachmentService : IAttachmentService
    {
        #region Private Read only properties

        private readonly IMapper _mapper;
        private readonly IAttachmentRepository _attachmentRepository;

        #endregion Private Read only properties

        #region Constructor

        public AttachmentService(IAttachmentRepository attachmentRepository, IMapper mapper)
        {
            _mapper = mapper;
            _attachmentRepository = attachmentRepository;
        }

        #endregion Constructor

        #region Public Methods

        public async Task<List<T>> UploadFiles<T>(List<BaseAttachment> attachments)
        {
            if (!attachments.Select(a => a.Attachment).ToList().Validate())
            {
                throw new ApiException(ErrorResponse.ErrorEnum.UploadFileError);
            }

            var result = new List<T>();
            foreach (var item in attachments)
            {
                result.Add(_mapper.Map<T>(await _attachmentRepository.UploadBlob(item.Attachment, item)));
            }

            return result;
        }

        public async Task<List<T>> ListOfFiles<T>(int principalId)
        {
            var attachments = await _attachmentRepository.ListOfFiles(principalId);
            return _mapper.Map<List<BaseAttachment>, List<T>>(attachments);
        }

        public Stream DownloadFile(int principalId, int attachmentId, string accessToken, out string fileName, out string contentType)
        {
            if (principalId <= 0 || attachmentId <= 0 || accessToken == null)
            {
                throw new ApiException(ErrorResponse.ErrorEnum.Validation);
            }

            return _attachmentRepository.GetFile(principalId, attachmentId, accessToken, out fileName, out contentType);
        }

        public async Task DeleteBlob(int principalId, int attachmentId)
        {
            if (principalId <= 0 || attachmentId <= 0)
            {
                throw new ApiException(ErrorResponse.ErrorEnum.Validation);
            }

            var fileDeleted = await _attachmentRepository.Delete(principalId, attachmentId);

            if (!fileDeleted)
            {
                throw new ApiException(ErrorResponse.ErrorEnum.NotFound);
            }
        }

        public async Task DeleteRange(int principalId, List<int> attachmentIds)
        {
            if (principalId <= 0 || attachmentIds == null || attachmentIds.Count <= 0)
            {
                throw new ApiException(ErrorResponse.ErrorEnum.Validation);
            }

            var fileDeleted = await _attachmentRepository.DeleteMultiple(principalId, attachmentIds);

            if (!fileDeleted)
            {
                throw new ApiException(ErrorResponse.ErrorEnum.NotFound);
            }
        }

        #endregion Public Methods
    }
}