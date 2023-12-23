using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShittyOne.Entities;

public class Survey
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public Guid? FileId { get; set; }
    public File? File { get; set; }
    public string JsonContent { get; set; }

    public List<SurveyQuestion> Questions { get; set; } = new();
    public Guid SurveyGroupId { get; set; }
}

public class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
    public void Configure(EntityTypeBuilder<Survey> builder)
    {
        builder.HasOne(s => s.File)
            .WithMany()
            .HasForeignKey(s => s.FileId)
            .OnDelete(DeleteBehavior.NoAction);

        //NoAction т.к. будем делать несколько привязок, а собирать мусорные файлы через задачу по расписанию (QuartzNET/Hangfire).
    }
}