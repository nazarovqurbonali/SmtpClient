using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using WebApi.Data.DTOs.EmailDto;

namespace WebApi.Services.EmailService;

public class EmailService(EmailConfiguration configuration) : IEmailService
{
    public void SendEmail(MessageDto message, TextFormat format)
    {
        var emailMessage = CreateEmailMessage(message, format);
        Send(emailMessage);
    }

    private MimeMessage CreateEmailMessage(MessageDto message, TextFormat format)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("mail", configuration.From));
        emailMessage.To.AddRange(message.To);
        emailMessage.Subject = message.Subject;
        emailMessage.Body = new TextPart(format) { Text = message.Content };

        return emailMessage;
    }

    private void Send(MimeMessage mailMessage)
    {
        using var client = new SmtpClient();
        try
        {
            client.Connect(configuration.SmtpServer, configuration.Port, true);
            client.AuthenticationMechanisms.Remove("OAUTH2");
            client.Authenticate(configuration.UserName, configuration.Password);

            client.Send(mailMessage);
        }
        finally
        {
            client.Disconnect(true);
            client.Dispose();
        }
    }
}