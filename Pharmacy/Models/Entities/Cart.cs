﻿namespace Pharmacy.Models.Entities;

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}