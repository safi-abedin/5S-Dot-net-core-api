using Microsoft.AspNetCore.Http;

namespace api.Services.Interfaces.Files
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(IFormFile file, string folderName);
        Task<List<string>> SaveManyAsync(IEnumerable<IFormFile>? files, string folderName);
    }
}
