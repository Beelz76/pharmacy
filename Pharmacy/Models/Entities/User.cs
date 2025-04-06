﻿namespace Pharmacy.Models.Entities;

public class User
{
    public int Id { get; set; }
    //public UserRoleEnum RoleId { get; set; }
    public string? Fullname { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    //public Role Role { get; set; } = default!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Favorites> Favorites { get; set; } = new List<Favorites>();
    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
}