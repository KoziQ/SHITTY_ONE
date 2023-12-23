using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;
using ShittyOne.Models;
using Microsoft.Extensions.Options;
using ShittyOne.Data;
using Microsoft.EntityFrameworkCore;
using ShittyOne.Entities;
using Hangfire;

namespace ShittyOne.Hangfire.Jobs
{
    public class EmailSurveyJob : IRecurringJob
    {
        public string CronExpression => Cron.Hourly();

        public string JobId => nameof(EmailSurveyJob);

        private readonly ImapEmailOptions _emailOptions;
        private readonly AppDbContext _dbContext;

        public EmailSurveyJob(IOptions<ImapEmailOptions> options, AppDbContext dbContext)
        {
            _emailOptions = options.Value;
            _dbContext = dbContext;
        }

        public async Task Execute()
        {
            var messages = await GetMessages();

            foreach (var message in messages)
            {
                var userAdress = (message.From.Mailboxes.FirstOrDefault()?.Address ?? "");
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userAdress);

                if (user == null)
                {
                    continue;
                }

                var survey = await _dbContext.Surveys
                    .Include(s => s.Questions.Where(q => q.Users.Any(u => u.Id == user.Id)).OrderBy(s => s.Title))
                    .ThenInclude(q => q.File)
                    .Include(s => s.Questions)
                    .ThenInclude(l => l.Answers.OrderBy(a => a.Text))
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(s => s.Id.ToString().ToLower() == message.Subject);

                if (survey == null)
                {
                    continue;
                }

                string stringToParse = message.TextBody;

                if (message.TextBody.EndsWith("\r\n"))
                {
                    stringToParse = message.TextBody.Substring(0, message.TextBody.Length - "\r\n".Length);
                }

                var result = await CompleteSurvey(user, survey, stringToParse);
            }
        }

        private async Task<bool> CompleteSurvey(User user, Survey survey, string body)
        {
            var answers = body.Split("\r\n");
            if (answers.Count() != survey.Questions.Count)
            {
                return false;
            }

            var userSession = new SurveySession { Survey = survey, User = user };

            foreach (var (question, index) in survey.Questions.Select((q, i) => (q, i)))
            {
                switch (question.Type)
                {
                    case SurveyQuestionType.Single:
                    case SurveyQuestionType.Multiple:
                    {
                        foreach (var answer in answers[index].Replace(" ", "").Split(','))
                        {
                            if (!int.TryParse(answer, out var questionIndex) || questionIndex > question.Answers.Count)
                            {
                                return false;
                            }

                            userSession.Answers.Add(new UserAnswer
                            {
                                Question = question,
                                Answer = question.Answers[questionIndex - 1],
                            });
                        }

                        break;
                    }
                    case SurveyQuestionType.Text:
                    {
                        userSession.Answers.Add(new UserAnswer { Question = question, TextAnswer = answers[index]! });
                        break;
                    }
                }

                return false;
            }

            userSession.End = DateTime.Now;
            _dbContext.SurveySessions.Add(userSession);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        private async Task<List<MimeMessage>> GetMessages()
        {
            using var client = new ImapClient();

            await client.ConnectAsync(_emailOptions.Host, _emailOptions.Port, _emailOptions.UseSsl);

            client.AuthenticationMechanisms.Remove("XOAUTH2");
            await client.AuthenticateAsync(_emailOptions.Email, _emailOptions.Password);

            var tmp = new List<MimeMessage>();

            await client.Inbox.OpenAsync(MailKit.FolderAccess.ReadWrite);

            foreach (var msg in client.Inbox.Fetch(client.Inbox.Search(SearchQuery.NotSeen),
                         MessageSummaryItems.Full | MessageSummaryItems.BodyStructure))
            {
                var message = client.Inbox.GetMessage(msg.Index);
                await client.Inbox.AddFlagsAsync(msg.Index, MessageFlags.Seen, true);
                tmp.Add(message);
            }

            await client.Inbox.CloseAsync();

            return tmp;
        }
    }
}