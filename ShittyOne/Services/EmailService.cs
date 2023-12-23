using FluentEmail.Core;
using System.Dynamic;
using System.Reflection;

namespace ShittyOne.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmail(string template, string to, string subject, object model);
    }
    public class EmailService : IEmailService
    {
        private const string TemplatePath = "ShittyOne.Resources.Emails.{0}.cshtml";
        private readonly IFluentEmailFactory _emailFactory;

        public EmailService(IFluentEmailFactory emailFactory)
        {
            _emailFactory = emailFactory;
        }

        public async Task<bool> SendEmail(string template, string to, string subject, object model)
        {
            var result = await _emailFactory.Create()
                .To(to)
                .Subject(subject)
                .UsingTemplateFromEmbedded(string.Format(TemplatePath, template), ToExpando(model), GetType().Assembly)
                .SendAsync();

            return result.Successful;
        }

        private ExpandoObject ToExpando(object model)
        {
            if (model is ExpandoObject exp)
            {
                return exp;
            }

            IDictionary<string, object> expando = new ExpandoObject()!;

            foreach (var prop in model.GetType().GetTypeInfo().GetProperties())
            {
                var obj = prop.GetValue(model);

                if (obj != null && obj.GetType().FullName.Contains("AnonymousType"))
                {
                    obj = ToExpando(obj);
                }

                expando.Add(prop.Name, obj!);
            }

            return (ExpandoObject)expando!;
        }
    }
}
