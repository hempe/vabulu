using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Vabulu.Services
{
    public class MailServiceOptions
    {
        public string ApiKey { get; set; }
        public string FromEmailAddress { get; set; }
        public string FromName { get; set; }
    }

    public class MailService
    {

        private readonly MailServiceOptions options;
        private readonly SendGridClient client;
        private readonly TemplateService templateService;

        public MailService(IOptions<MailServiceOptions> options, TemplateService templateService)
        {
            this.options = options.Value;
            this.client = new SendGridClient(this.options.ApiKey);
            this.templateService = templateService;
        }

        public async Task SendEmailAsync(string receiverEmail, string subject, string templatePath, object data, string language = null)
        {

            var template = await this.templateService.LoadTemplateAsync(templatePath);
            var content = this.templateService.Render(template, data, language);
            var email = new SendGridMessage
            {
                From = new EmailAddress(this.options.FromEmailAddress, this.options.FromName),
                Subject = subject,
                HtmlContent = content
            };

            email.AddTo(receiverEmail);
            var response = await this.client.SendEmailAsync(email);
        }

        public async Task<string> ParseSendEmailAsync(string receiverEmail, string subject, string templatePath, object data, string language = null)
        {

            var template = await this.templateService.LoadTemplateAsync(templatePath);
            return this.templateService.Render(template, data, language);
        }
    }
}