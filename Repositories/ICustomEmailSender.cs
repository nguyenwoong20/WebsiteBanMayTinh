namespace Website_BanMayTinh.Repositories
{
    public interface ICustomEmailSender 
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
