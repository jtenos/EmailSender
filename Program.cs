using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;

namespace EmailSender
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            string fileName = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName,
                args[0]);
            var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(fileName));

            using var smtp = new SmtpClient(config["EmailHost"]);
            int? smtpPort = config.GetValue<int?>("SmtpPort");
            if (smtpPort.HasValue)
            {
                smtp.Port = smtpPort.Value;
            }
            int? smtpSecure = config.GetValue<int?>("SmtpSecure");
            if (smtpSecure.HasValue)
            {
                smtp.EnableSsl = smtpSecure == 1;
            }
            string? smtpUserName = config["SmtpUserName"];
            string? smtpPassword = config["SmtpPassword"];
            if (!string.IsNullOrWhiteSpace(smtpUserName) && !string.IsNullOrWhiteSpace(smtpPassword))
            {
                smtp.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
            }

            var msg = new MailMessage
            {
                From = new MailAddress(message.SenderEmail, message.SenderName),
                Subject = message.Subject,
                IsBodyHtml = true,
                Body = message.HtmlBody
            };
            foreach (var rec in message.Recipients) { msg.To.Add(new MailAddress(rec.Email, rec.Name)); }
            await smtp.SendMailAsync(msg).ConfigureAwait(false);

        }
    }

    public class Message
    {
        public string SenderName { get; set; } = default!;
        public string SenderEmail { get; set; } = default!;
        public Recipient[] Recipients { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string HtmlBody { get; set; } = default!;
    }

    public class Recipient
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
