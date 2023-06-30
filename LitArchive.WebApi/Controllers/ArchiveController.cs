using LBox.Common.WebApi.Attributes;
using LitArchive.Infrastructure.Contracts;
using LitArchive.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace LitArchive.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArchiveController : ControllerBase
    {
        public const string DEFAULT_CONTENT_TYPE = "application/octet-stream";
        public const string POSTER_FOLDER = "posters";
        public const string POSTER_DEFAULT = "default.jpg";

        protected IWebHostEnvironment _webHostEnvironment;
        protected IArchiveDataService _dataService;

        public ArchiveController(IWebHostEnvironment webHostEnvironment, IArchiveDataService dataService)
        {
            _webHostEnvironment = webHostEnvironment;
            _dataService = dataService;
        }

        [Authorize]
        [HttpPost("access")]
        public string GetAccessKey()
        {
            return _dataService.GetAccessKey();
        }

        [Authorize]
        [HttpGet]
        public ArchiveData GetArchiveData(string path = "")
        {
            return _dataService.GetArchiveData(path);
        }

        [HttpGet("poster")]
        public PhysicalFileResult GetPoster(string path, string access)
        {
            try
            {
                if (!_dataService.ValidateAccessKey(access))
                    return ReturnDefaultPoster();

                var folder = Path.Combine(_webHostEnvironment.ContentRootPath, POSTER_FOLDER);
                var posterPath = _dataService.GetVideoPoster(folder, path);
                if (String.IsNullOrWhiteSpace(posterPath))
                    return ReturnDefaultPoster();

                return PhysicalFile(posterPath, this.GetMimeTypeForFileExtension(posterPath));
            }
            catch
            {
                return ReturnDefaultPoster();
            }
        }

        protected PhysicalFileResult ReturnDefaultPoster()
        {
            var defaultPath = Path.Combine(_webHostEnvironment.ContentRootPath, POSTER_FOLDER, POSTER_DEFAULT);
            return PhysicalFile(defaultPath, this.GetMimeTypeForFileExtension(defaultPath));
        }

        //[HttpGet("file/{key}")]
        //public PhysicalFileResult GetFile(string key, string path)
        //{

        //    var filePath = _dataService.GetFullPath(path);
        //    if (!_dataService.ValidateAccessKey(key, path))
        //        throw new CustomAccessException("Access key is invalid");
        //    return PhysicalFile(filePath, this.GetMimeTypeForFileExtension(filePath));
        //}

        protected string GetMimeTypeForFileExtension(string path)
        {
            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(path, out string contentType))
            {
                contentType = DEFAULT_CONTENT_TYPE;
            }

            return contentType;
        }
    }
}
