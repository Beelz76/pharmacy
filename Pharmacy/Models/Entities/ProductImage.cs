﻿namespace Pharmacy.Models.Entities;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Product Product { get; set; } = default!;
}