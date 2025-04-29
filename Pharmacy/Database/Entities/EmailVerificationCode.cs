using Pharmacy.Shared.Enums;

namespace Pharmacy.Database.Entities;

public class EmailVerificationCode
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; } = default!;
    public string Code { get; set; } = default!;
    public VerificationPurposeEnum Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    
    public User User { get; set; } = default!;
}