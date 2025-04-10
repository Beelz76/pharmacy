﻿namespace Pharmacy.Models.Entities;

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    
    public User User { get; set; } = default!;
    public Role Role { get; set; } = default!;
}