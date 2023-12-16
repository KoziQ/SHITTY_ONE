﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace ShittyOne.Entities
{
    //TPH - base class
    public abstract class SurveyQuestion
    {
        public Guid Id { get; set; }
        public int Position { get; set; }
        public string Title { get; set; }
        public string JsonContent { get; set; }
        public Guid SurveyId { get; set; }
        public Survey Survey { get; set; }
        public Guid? FileId { get; set; }
        public File? File { get; set; }
        public List<Group> Groups { get; set; } = new List<Group>();
        public List<User> Users { get; set; } = new List<User>();
    }

    [Display(Name = "Вопрос с множественны выбором")]
    public class MultipleQuestion : SurveyQuestion
    {
        public List<SurveyQuestionAnswer> Answers { get; set; } = new List<SurveyQuestionAnswer>();
    }

    [Display(Name = "Вопрос со свободным варинатом ответа")]
    public class StringQuestion : SurveyQuestion
    {
        
    }

    public class BaseClassConfiguration : IEntityTypeConfiguration<SurveyQuestion>
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
        }
    }

    public class MultipleQuestionConfiguration : IEntityTypeConfiguration<MultipleQuestion>
    {
        public void Configure(EntityTypeBuilder<MultipleQuestion> builder)
        {
            builder.HasMany(s => s.Answers)
                .WithOne(s => s.Question as MultipleQuestion)
                .HasForeignKey(s => s.SuveyQuestionId)
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
