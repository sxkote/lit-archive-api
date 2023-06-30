using LitArchive.Infrastructure.Enums;

namespace LitArchive.Infrastructure.Models
{
    public class ArchiveItem
    {
        public ArchiveItemType Type { get; set; }
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Extension { get; set; }
        public long? Size { get; set; }

        public ArchiveItem(ArchiveItemType type, string path, string name)
        {
            this.Type = type;
            this.Path = path;
            this.Name = name;
        }
    }
}
