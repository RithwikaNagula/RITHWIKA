namespace Application.Interfaces.Services
{
    public interface IRecaptchaService
    {
        Task<bool> VerifyAsync(string token);
    }
}
