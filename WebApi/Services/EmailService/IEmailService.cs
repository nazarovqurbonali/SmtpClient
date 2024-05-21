using MimeKit.Text;
using WebApi.Data.DTOs.EmailDto;

namespace WebApi.Services.EmailService;

public interface IEmailService
{
    void SendEmail(MessageDto model,TextFormat format);
}