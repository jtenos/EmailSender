using System.IO;
using System.Reflection;
using System.Text.Json;
using EmailSender;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();
string fileName = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName,
    args[0]);
var message = JsonSerializer.Deserialize<Message>(File.ReadAllText(fileName))!;

string host = config["EmailHost"];
int port = config.GetValue<int>("SmtpPort");
string userName = config["SmtpUserName"];
string password = config["SmtpPassword"];

MimeMessage msg = new();
foreach (var rec in message.Recipients)
{
    msg.To.Add(new MailboxAddress(rec.Name, rec.Email));
}
msg.From.Add(new MailboxAddress(message.SenderName, message.SenderEmail));
msg.Subject = message.Subject;

var bodyBuilder = new BodyBuilder();
bodyBuilder.HtmlBody = message.HtmlBody;
bodyBuilder.TextBody = message.TextBody;

msg.Body = bodyBuilder.ToMessageBody();

using var emailClient = new SmtpClient();

switch (port)
{
    case 25:
        await emailClient.ConnectAsync(host, port: 25, options: SecureSocketOptions.None);
        break;
    case 465:
        await emailClient.ConnectAsync(host, port, useSsl: true).ConfigureAwait(false);
        break;
    case 587:
        await emailClient.ConnectAsync(host, port, options: SecureSocketOptions.StartTls);
        break;
}

await emailClient.AuthenticateAsync(userName, password).ConfigureAwait(false);
await emailClient.SendAsync(msg).ConfigureAwait(false);
await emailClient.DisconnectAsync(true).ConfigureAwait(false);
