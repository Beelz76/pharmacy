﻿namespace Pharmacy.Models.Entities;

public class EmailVerificationCode
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    
    public User User { get; set; } = default!;
}