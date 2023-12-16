using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShittyOne.Entities
{
    public class UserAnswer
    {
        public Guid Id { get; set; }
        public Guid? SurveyQuestionAnswerId { get; set; }
        public SurveyQuestionAnswer? Answer { get; set; }
        public Guid QuestionId { get; set; }
        public SurveyQuestion Question { get; set;}
        public string? TextAnswer { get; set; }
        public Guid SessionId { get; set; }
        public UserSession Session { get; set; }
    }

    public class UserAnswerConfiguration : IEntityTypeConfiguration<UserAnswer>
    {
        public void Configure(EntityTypeBuilder<UserAnswer> builder)
        {
            builder.HasOne(s => s.Answer)
                .WithMany()
                .HasForeignKey(s => s.SurveyQuestionAnswerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(s => s.Session)
                .WithMany(s => s.Answers)
                .HasForeignKey(s => s.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Question)
                .WithMany()
                .HasForeignKey(q => q.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
