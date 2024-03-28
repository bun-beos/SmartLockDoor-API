using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace SmartLockDoor
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void SendEmail(EmailDto emailDto)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSettings:EmailUsername").Value));
            email.To.Add(MailboxAddress.Parse(emailDto.To));
            email.Subject = emailDto.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = emailDto.Body };

            var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EmailSettings:EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration.GetSection("EmailSettings:EmailUsername").Value, _configuration.GetSection("EmailSettings:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
