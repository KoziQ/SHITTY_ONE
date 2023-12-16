using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShittyOne.Entities;
using File = ShittyOne.Entities.File;

namespace ShittyOne.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public virtual DbSet<UserRefresh> Refreshes { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<File> Files { get; set; }

        public virtual DbSet<Survey> Surveys { get; set; }
        public virtual DbSet<MultipleQuestion> MultipleQuestions { get; set; }
        public virtual DbSet<StringQuestion> StringQuestions { get; set; }
        public virtual DbSet<SurveyQuestionAnswer> SurveysAnswer { get; set; }
        public virtual DbSet<UserAnswer> UserAnswers { get; set; }
        public virtual DbSet<UserSession> UserSessions { get; set; }

        public AppDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new UserRefreshConfiguration());
            builder.ApplyConfiguration(new GroupConfigurations());
            builder.ApplyConfiguration(new SurveyConfiguration());
            builder.ApplyConfiguration(new UserAnswerConfiguration());
            builder.ApplyConfiguration(new UserSessionConfiguration());
            builder.ApplyConfiguration(new BaseClassConfiguration());
            builder.ApplyConfiguration(new MultipleQuestionConfiguration());

            base.OnModelCreating(builder);
        }
    }
}
