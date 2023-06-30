using FFMpegCore;
using LBox.Common.Shared;
using LBox.Common.Shared.Contracts;
using LBox.Common.Shared.Exceptions;
using LitArchive.Infrastructure.Contracts;
using LitArchive.Infrastructure.Enums;
using LitArchive.Infrastructure.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text;

namespace LitArchive.Infrastructure.Services
{
    public class ArchiveDataService : IArchiveDataService
    {
        //public const string ROOT = "/root";

        protected ArchiveOptions _options;
        protected IAuthTokenProvider _authTokenProvider;
        protected IMemoryCache _cache;
        protected string Root => _options.Root;

        public ArchiveDataService(IOptions<ArchiveOptions> options, IAuthTokenProvider authTokenProvider, IMemoryCache cache)
        {
            _options = options.Value;
            _authTokenProvider = authTokenProvider;
            _cache = cache;
        }

        public string GetAccessKey(string? path = null)
        {
            var authToken = _authTokenProvider.GetToken();
            if (authToken == null)
                throw new CustomAuthenticationException("User not authorized");

            var key = Guid.NewGuid().ToString();
            var value = authToken.UserID.ToString();
            if (!String.IsNullOrWhiteSpace(path))
                value = value + ":" + path;

            _cache.Set(key, value, TimeSpan.FromMinutes(_options.CacheTimespanInMinutes));

            return key;
        }

        public bool ValidateAccessKey(string key, string? path = null)
        {
            if (!_cache.TryGetValue<string>(key, out string? value))
                return false;

            if (String.IsNullOrWhiteSpace(value))
                return false;

            if (!String.IsNullOrEmpty(path))
            {
                var values = value.Split(":");
                if (values == null || values.Length != 2 || String.IsNullOrWhiteSpace(values[1]))
                    return false;

                return path.StartsWith(values[1]);
            }

            return !String.IsNullOrWhiteSpace(value);
        }


        public ArchiveData GetArchiveData(string path)
        {
            return new ArchiveData()
            {
                Items = this.GetArchiveItems(path)
            };
        }

        public List<ArchiveItem> GetArchiveItems(string path)
        {
            var result = new List<ArchiveItem>();

            var directory = new DirectoryInfo(GetFullPath(path));

            var folders = directory.GetDirectories().Where(d => FilterSkipped(d));
            result.AddRange(folders.Select(dir => new ArchiveItem(ArchiveItemType.Folder, GetRelativePath(dir.FullName), dir.Name)));

            var files = directory.GetFiles().Where(f => FilterSkipped(f));
            result.AddRange(files.Select(file => new ArchiveItem(ArchiveItemType.File, GetRelativePath(file.FullName), file.Name)
            {
                Extension = file.Extension,
                Size = file.Length
            }));

            return result;
        }

        protected bool FilterSkipped(DirectoryInfo directory)
        {
            if (directory.Name.StartsWith("#") || directory.Name.StartsWith("@"))
                return false;

            if (_options.SkipFolderName != null)
                if (_options.SkipFolderName.Any(sk => directory.Name.Equals(sk, StringComparison.OrdinalIgnoreCase)))
                    return false;

            if (_options.SkipPath != null)
                if (_options.SkipPath.Any(sk => directory.FullName.StartsWith(sk, StringComparison.OrdinalIgnoreCase)))
                    return false;

            return true;
        }

        protected bool FilterSkipped(FileInfo file)
        {
            if (_options.SkipFileName != null)
                if (_options.SkipFileName.Any(sk => file.Name.Equals(sk, StringComparison.OrdinalIgnoreCase)))
                    return false;

            if (_options.SkipPath != null)
                if (_options.SkipPath.Any(sk => file.FullName.StartsWith(sk, StringComparison.OrdinalIgnoreCase)))
                    return false;

            return true;
        }

        public string GetFullPath(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                return this.Root;

            if (!path.StartsWith(this.Root, StringComparison.OrdinalIgnoreCase))
                return this.Root + path;

            return path;
        }

        protected string GetRelativePath(string path)
        {
            if (path.Equals(this.Root, StringComparison.OrdinalIgnoreCase))
                return "/";

            if (path.StartsWith(this.Root, StringComparison.OrdinalIgnoreCase))
                return path.Substring(this.Root.Length);

            return path;
        }

      
        public string GetVideoPoster(string posterFolder, string videoFilePath)
        {
            var hash = Helpers.CalculateMD5(Encoding.Default.GetBytes(videoFilePath)) + "-" + videoFilePath.Length.ToString();
            var posterPath = Path.Combine(posterFolder, $"{hash}.png");
            if (!File.Exists(posterPath))
                FFMpeg.Snapshot(this.GetFullPath(videoFilePath), posterPath, captureTime: TimeSpan.FromSeconds(1));
            return posterPath;
        }

    }
}
