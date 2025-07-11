﻿using FastEndpoints;
using FluentValidation;
using Pharmacy.Extensions;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto.Cart;

namespace Pharmacy.Endpoints.Cart;

public class AddRangeEndpoint : Endpoint<AddCartItemsRequest>
{
    private readonly ICartService _cartService;

    public AddRangeEndpoint(ICartService cartService)
    {
        _cartService = cartService;
    }

    public override void Configure()
    {
        Post("cart/bulk");
        Roles("User");
        Tags("Cart");
        Summary(s => { s.Summary = "Добавить несколько товаров в корзину"; });
    }

    public override async Task HandleAsync(AddCartItemsRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await _cartService.AddRangeAsync(userId.Value, req.Items);
        if (result.IsSuccess)
        {
            await SendOkAsync(ct);
        }
        else
        {
            await SendAsync(result.Error, (int)result.Error.StatusCode, ct);
        }
    }
}

public record AddCartItemsRequest(List<CartItemQuantityDto> Items);

public class AddCartItemsRequestValidator : Validator<AddCartItemsRequest>
{
    public AddCartItemsRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}