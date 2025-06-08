namespace Pharmacy.Shared.Dto.Cart;

public record CartDto(List<CartItemDto> Items, decimal TotalPrice);