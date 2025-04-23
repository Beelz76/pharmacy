namespace Pharmacy.Endpoints.Users.Verification;

public class CodeGenerator
{
    private static readonly Random _random = new();

    public string GenerateDigits(int length)
    {
        return string.Concat(Enumerable.Range(0, length).Select(_ => _random.Next(0, 10)));
    }
}