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

        public string GetPasswordTokenBody(string passwordToken)
        {
            var body = "<head> <style> img {width:64px; height:64px; border-radius:10px; margin-bottom:12px;} .content {text-align:center;}.content p {text-align:center;} p {font-size: 16px !important;} b {font-size:40px !important; display:inline-block; margin:36px 0 60px; letter-spacing: 0.3em;}  </style> </head> <body> <p>Xin chào,</p> <div class=\"content\"> <img src=\"https://firebasestorage.googleapis.com/v0/b/smart-doorbell-ffebe.appspot.com/o/Smart%20Lock%20Door%2FApp%2Fapp_logo.png?alt=media&token=a6bf0bc1-7240-4432-a7d8-e6b89c0f1ed6\" alt=\"Logo\"> <p>Đây là mã xác thực quên mật khẩu của bạn.</p> <b>%token%</b> <p>Mã xác thực này sẽ hết hiệu lực sau 3 phút. Vui lòng ấn quên mật khẩu để nhận mã xác thực mới.</p> <p>Vui lòng không trả lời email này.</p> </div> </body>";

            return body.Replace("%token%", passwordToken);
        }

        public string GetVerifyTokenBody(string verifyUrl)
        {
            var body = "<head> <style> img {width:64px; height:64px; border-radius:10px; margin-bottom:12px;} .content {text-align:center;}.content p {text-align:center;} p {font-size: 16px !important;} a {display:inline-block; text-decoration:none; color:#fff !important; font-size:14px; background-color:#25a3f5; width:200px; height:60px; line-height:60px; border-radius:14px; margin:36px 0 60px;} a:hover {background-color:#20ddff;}  </style> </head> <body> <p>Xin chào,</p> <div class=\"content\"> <img src=\"https://firebasestorage.googleapis.com/v0/b/smart-doorbell-ffebe.appspot.com/o/Smart%20Lock%20Door%2FApp%2Fapp_logo.png?alt=media&token=a6bf0bc1-7240-4432-a7d8-e6b89c0f1ed6\" alt=\"Logo\"> <p>Hãy xác thực tài khoản của bạn.</p> <a href=\"%link%\">Ấn để xác thực tài khoản</a> <p>Email xác thực có hiệu lực trong vòng 1 tiếng. Sau 1 tiếng vui lòng đăng ký lại để nhận email xác thực mới.</p> <p>Vui lòng không trả lời email này.</p> </div> </body>";

            return body.Replace("%link%", verifyUrl);
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
