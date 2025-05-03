namespace Pharmacy.Shared.Dto;

public record CartDto(List<CartItemDto> Items, decimal TotalPrice);