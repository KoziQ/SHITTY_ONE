using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShittyOne.Entities;

public enum SurveyQuestionType
{
    Single,
    Multiple,
    Text
}

public class SurveyQuestion
{
    public Guid Id { get; set; }
    public SurveyQuestionType Type { get; set; }
    public int Position { get; set; }

    public string Title { get; set; }

    public Guid SurveyId { get; set; }
    public Survey Survey { get; set; }

    public Guid? FileId { get; set; }
    public File? File { get; set; }

    public List<Group> Groups { get; set; } = new();
    public List<User> Users { get; set; } = new();

    public List<SurveyQuestionAnswer> Answers { get; set; } = new();
}

public class SurveyQuestionConfiguration : IEntityTypeConfiguration<SurveyQuestion>
{
    public void Configure(EntityTypeBuilder<SurveyQuestion> builder)
    {
        builder.HasOne(s => s.Survey)
            .WithMany(s => s.Questions)
            .HasForeignKey(s => s.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.File)
            .WithMany()
            .HasForeignKey(s => s.FileId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(s => s.Users)
            .WithMany();

        builder.HasMany(s => s.Groups)
            .WithMany();

        builder.HasMany(s => s.Answers)
            .WithOne(s => s.Question)
            .HasForeignKey(s => s.SuveyQuestionId)
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}