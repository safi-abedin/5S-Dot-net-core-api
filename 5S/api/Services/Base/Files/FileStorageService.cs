using api.Services.Interfaces.Files;
using Microsoft.AspNetCore.Http;

namespace api.Services.Base.Files
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileStorageService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> SaveAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Invalid file");

            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var uploadFolder = Path.Combine(webRootPath, "uploads", folderName);
            Directory.CreateDirectory(uploadFolder);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadFolder, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"uploads/{folderName}/{fileName}";
            var request = _httpContextAccessor.HttpContext?.Request;

            if (request == null)
                return $"/{relativePath}";

            return $"{request.Scheme}://{request.Host}/{relativePath}";
        }

        public async Task<List<string>> SaveManyAsync(IEnumerable<IFormFile>? files, string folderName)
        {
            var urls = new List<string>();

            if (files == null)
                return urls;

            foreach (var file in files.Where(x => x != null && x.Length > 0))
            {
                var url = await SaveAsync(file, folderName);
                urls.Add(url);
            }

            return urls;
        }
    }
}
