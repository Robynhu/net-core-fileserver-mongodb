using System.IO;

namespace Contracts.Models
{
    public class FileModel
    {
        public string Id { get; set; }      
        public Stream FileStream { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
