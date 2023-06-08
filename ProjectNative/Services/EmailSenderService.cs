using Microsoft.Extensions.Options;
using ProjectNative.Services.IService;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;

namespace ProjectNative.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IOptions<EmailSenderOptions> _options;

        public EmailSenderService(IOptions<EmailSenderOptions>options )
        {
            _options = options;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(_options.Value.SendGridKey, subject, htmlMessage, email);
        }

        private Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_options.Value.SendGridEmail, _options.Value.SendGridName),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            return client.SendEmailAsync(msg);
        }

        public class EmailSenderOptions
        {
            public string SendGridKey { get; set; }
            public string SendGridEmail { get; set; }
            public string SendGridName { get; set; }
        }
    }

}
