using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShittyOne.Entities;

public class UserSession
{
    public Guid Id { get; set; }
    public DateTime Start { get; private set; } = DateTime.Now;
    public DateTime? End { get; set; }
    public List<UserAnswer> Answers { get; set; } = new();
    public User User { get; set; }
    public Guid UserId { get; set; }
    public Survey Survey { get; set; }
    public Guid SurveyId { get; set; }
}

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Survey)
            .WithMany()
            .HasForeignKey(s => s.SurveyId)
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}