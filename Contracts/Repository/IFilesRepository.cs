using System.IO;
using Contracts.Models;

namespace Contracts.Repository
{
    public interface IFilesRepository
    {
        string AddFile(Stream fileStream, string fileName, string contentType, string userId);
        FileModel GetFile(string objectId, string userId);

    }
}