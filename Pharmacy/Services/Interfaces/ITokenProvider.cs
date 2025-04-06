namespace Pharmacy.Services.Interfaces;

public interface ITokenProvider
{
    string Create(int userId, string email, string Role);
}