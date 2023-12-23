using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShittyOne.Entities;
using File = ShittyOne.Entities.File;

namespace ShittyOne.Data;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public virtual DbSet<UserRefresh> Refreshes { get; set; }
    public virtual DbSet<Group> Groups { get; set; }
    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<Survey> Surveys { get; set; }

    public virtual DbSet<SurveyQuestion> SurveyQuestions { get; set; }

    public virtual DbSet<SurveyQuestionAnswer> SurveysAnswer { get; set; }
    public virtual DbSet<UserAnswer> UserAnswers { get; set; }
    public virtual DbSet<SurveySession> SurveySessions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new UserRefreshConfiguration());
        builder.ApplyConfiguration(new GroupConfigurations());
        builder.ApplyConfiguration(new SurveyConfiguration());
        builder.ApplyConfiguration(new UserAnswerConfiguration());
        builder.ApplyConfiguration(new SurveySessionConfiguration());
        builder.ApplyConfiguration(new SurveyQuestionConfiguration());

        base.OnModelCreating(builder);
    }
}