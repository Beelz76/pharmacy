﻿using Pharmacy.Endpoints.Users.Authorization;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services.Interfaces;

public interface IAuthorizationService
{
    Task<Result<string>> RegisterAsync(RegisterRequest request);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
}