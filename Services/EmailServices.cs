using System.Net;
using System.Net.Mail;
using System.Text;

namespace BuyZaar.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var settings = _config.GetSection("EmailSettings");

            var host = settings["Host"];
            var port = int.Parse(settings["Port"]!);
            var username = settings["Username"];
            var fromEmail = settings["FromEmail"];
            var fromName = settings["FromName"];

            var password = Environment.GetEnvironmentVariable("SMTP_PASS");

            var smtpClient = new SmtpClient(host)
            {
                Port = port,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail!, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            message.To.Add(toEmail);

            smtpClient.Send(message);
        }
    }
}