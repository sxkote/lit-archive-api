namespace LitArchive.Infrastructure.Models
{
    public class ArchiveOptions
    {
        public string Root { get; set; }
        public string[] SkipPath { get; set; }
        public string[] SkipFolderName { get; set; }
        public string[] SkipFileName { get; set; }
        public int CacheTimespanInMinutes { get; set; }
    }
}
