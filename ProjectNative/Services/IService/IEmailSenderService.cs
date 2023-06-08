namespace ProjectNative.Services.IService
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string email,string subject,string htmlMessage);
    }
}
