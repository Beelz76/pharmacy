namespace Pharmacy.Database.Entities;

public class Address
{
    public int Id { get; set; }
    public string? OsmId { get; set; }
    
    public string? Region { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? Suburb { get; set; }
    public string? Street { get; set; }
    public string? HouseNumber { get; set; }
    public string? Postcode { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public Pharmacy? Pharmacy { get; set; }
}