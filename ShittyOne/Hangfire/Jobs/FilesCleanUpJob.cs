using Hangfire;
using Microsoft.EntityFrameworkCore;
using ShittyOne.Data;
using ShittyOne.Services;
using System.Runtime.CompilerServices;
using System.Text;

namespace ShittyOne.Hangfire.Jobs
{
    public class FilesCleanUpJob : IRecurringJob
    {
        public string CronExpression => Cron.Weekly();

        public string JobId => "FilesCleanUpJob";

        private readonly AppDbContext _dbContext;
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _hostEnvironment;

        public FilesCleanUpJob(AppDbContext dbContext, IFileService fileService, IWebHostEnvironment hostEnvironment)
        {
            _dbContext = dbContext;
            _fileService = fileService;
            _hostEnvironment = hostEnvironment;
        }

        public async Task Execute()
        {
            _dbContext.Files.RemoveRange(await GetJunkFiles());
            await _dbContext.SaveChangesAsync();

            //Delete files
            foreach (var file in Directory.GetFiles(Path.Combine(_hostEnvironment.WebRootPath, "uploads".TrimStart(Path.AltDirectorySeparatorChar)), "*", SearchOption.AllDirectories))
            {
                //Just in case I want to add some additional data to fileName before guid (Must be separated with _)...
                var filename = file.Replace(file.Split("\\").Last(), file.Split("\\").Last().Split('_').Last());

                if (!await _dbContext.Files.AnyAsync(f => filename.Replace(_hostEnvironment.WebRootPath, "").Replace("\\", "/") == f.SubDir))
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch (IOException exception)
                    {
                        //TODO Logging
                        //there's an open handle on the file.
                    }
                    catch (Exception exception)
                    {
                        //TODO Logging
                        //unexpected exception
                    }
                }
            }
        }

        private async Task<List<ShittyOne.Entities.File>> GetJunkFiles()
        {
            var filesQueryBuilder = new StringBuilder();
            filesQueryBuilder.AppendLine(@"SELECT * FROM [dbo].Files WHERE 1 = 1");

            //Getting all entities that have FileId property.
            var entityTypes = _dbContext.Model.GetEntityTypes().Where(t => t.FindProperty("FileId") != null).ToList();

            foreach (var type in entityTypes)
            {
                filesQueryBuilder.AppendLine($" AND Id NOT IN(SELECT ISNULL(FileId,0) FROM [dbo].{type.GetTableName()} WHERE FileId IS NOT NULL)");
            }

            //Distinct all files if any entity have the same IconId
            entityTypes = _dbContext.Model.GetEntityTypes().Where(t => t.FindProperty("IconId") != null).ToList();

            foreach (var type in entityTypes)
            {
                filesQueryBuilder.AppendLine($" AND Id NOT IN(SELECT ISNULL(IconId, 0) FROM [dbo].{type.GetTableName()} WHERE IconId IS NOT NULL)");
            }


            //Remove all files if any entity with json content property has "subdir" to file. (There's no way that guid + data folder + uploads will be in text)
            entityTypes = _dbContext.Model.GetEntityTypes().Where(t => t.FindProperty("JsonContent") != null).ToList();

            foreach (var type in entityTypes)
            {
                filesQueryBuilder.AppendLine($"AND NOT EXISTS(SELECT * FROM [dbo].{type.GetTableName()} WHERE JsonContent LIKE '%' + {nameof(ShittyOne.Entities.File.SubDir)} + '%')");
            }

            return await _dbContext.Files.FromSql(FormattableStringFactory.Create(filesQueryBuilder.ToString())).ToListAsync();
        }
    }
}
