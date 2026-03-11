// Service contract for validating Google reCAPTCHA tokens on public-facing forms.
namespace Application.Interfaces.Services
{
    public interface IRecaptchaService
    {
        // Sends the client-side reCAPTCHA token to Google's API and returns true if the response passes the score threshold
        Task<bool> VerifyAsync(string token);
    }
}
