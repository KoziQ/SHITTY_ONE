using Microsoft.EntityFrameworkCore;
using ShittyOne.Data;

namespace ShittyOne.Services
{
    public interface IFileService
    {
        public Task<Entities.File?> AddFile(IFormFile file);
        public Task<bool> RemoveFile(Guid file);
    }
    public class FileService : IFileService
    {
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _hostEnviroment;

        public FileService(AppDbContext dbContext, IWebHostEnvironment hostEnviroment)
        {
            _dbContext = dbContext;
            _hostEnviroment = hostEnviroment;
        }

        public async Task<Entities.File?> AddFile(IFormFile file)
        {
            if (GetContentType(file.ContentType) == null)
            {
                throw new FormatException("unsupported media type");
            }

            using (var sr = new StreamContent(file.OpenReadStream()))
            {
                var content = await sr.ReadAsByteArrayAsync();
                if (content == null)
                {
                    return null;
                }
                else
                {
                    var directory = $"/uploads/{DateTime.Now.ToString("dd_MM_yyyy")}/";
                    var workingFolder = Path.Combine(_hostEnviroment.WebRootPath, directory.TrimStart(System.IO.Path.AltDirectorySeparatorChar));

                    if (!Directory.Exists(workingFolder))
                    {
                        Directory.CreateDirectory(workingFolder);
                    }
                    var fileGuid = Guid.NewGuid().ToString();

                    await File.WriteAllBytesAsync(Path.Combine(workingFolder, $"{fileGuid}{GetContentType(file.ContentType)}"), content);

                    var dbFile = new Entities.File
                    {
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        SubDir = Path.Combine(directory, $"{fileGuid}{GetContentType(file.ContentType)}").Replace(@"\\", "/")
                    };

                    _dbContext.Files.Add(dbFile);

                    await _dbContext.SaveChangesAsync();

                    return dbFile;
                }
            }

        }

        public async Task<bool> RemoveFile(Guid fileId)
        {
            var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null)
            {
                return false;
            }

            if (!file.SubDir.StartsWith("/uploads"))
            {
                _dbContext.Files.Remove(file);
                await _dbContext.SaveChangesAsync();

                return true;
            }

            var filePath = Path.Combine(_hostEnviroment.WebRootPath, file.SubDir.TrimStart(System.IO.Path.AltDirectorySeparatorChar));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _dbContext.Files.Remove(file);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        //TODO: Add library for mime type
        private string GetContentType(string contentType)
        {
            switch (contentType)
            {
                case "image/jpeg":
                    return ".jpg";
                case "image/png":
                    return ".png";
                case "image/gif":
                    return ".gif";
                case "image/svg+xml":
                    return ".svg";
                case "application/pdf":
                    return ".pdf";
                case "application/vnd.ms-excel":
                    return ".xls";
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    return ".xlsx";
                case "video/mp4":
                    return ".mp4";
                case "video/mpeg":
                    return ".mpeg";
                case "audio/mpeg":
                    return ".mp3";
                case "audio/midi":
                    return ".mid";
                case "audio/x-midi":
                    return ".midi";
                case "video/x-msvideo":
                    return ".avi";
                case "application/msword":
                    return ".doc";
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    return ".docx";
            }

            return null;
        }
    }
}