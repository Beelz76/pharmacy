using Microsoft.EntityFrameworkCore;
using Pharmacy.Database.Entities;
using Pharmacy.Database.Repositories.Interfaces;
using Pharmacy.DateTimeProvider;
using Pharmacy.Endpoints.Users;
using Pharmacy.Endpoints.Users.Authentication;
using Pharmacy.Services.Interfaces;
using Pharmacy.Shared.Dto;
using Pharmacy.Shared.Dto.Address;
using Pharmacy.Shared.Dto.Pharmacy;
using Pharmacy.Shared.Dto.User;
using Pharmacy.Shared.Enums;
using Pharmacy.Shared.Result;

namespace Pharmacy.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly PasswordProvider _passwordProvider;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IPharmacyRepository _pharmacyRepository;
    public UserService(IUserRepository repository, IDateTimeProvider dateTimeProvider, PasswordProvider passwordProvider, IEmailVerificationService emailVerificationService, IPharmacyRepository pharmacyRepository)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
        _passwordProvider = passwordProvider;
        _emailVerificationService = emailVerificationService;
        _pharmacyRepository = pharmacyRepository;
    }

    public async Task<Result<CreatedDto>> CreateAsync(CreateUserDto dto)
    {
        if (dto.Role == UserRoleEnum.Employee && dto.PharmacyId is not null)
        {
            var exists = await _pharmacyRepository.ExistsByIdAsync(dto.PharmacyId.Value);
            if (!exists)
            {
                return Result.Failure<CreatedDto>(Error.NotFound("Указанная аптека не найдена"));
            }
        }
        
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = dto.PasswordHash,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Patronymic = dto.Patronymic,
            Phone = dto.Phone,
            EmailVerified = dto.EmailVerified,
            Role = dto.Role,
            PharmacyId = dto.PharmacyId,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow
        };
        
        await _repository.AddAsync(user);
        return Result.Success(new CreatedDto(user.Id));
    }

    public async Task<Result> UpdateProfileAsync(int id, UpdateProfileRequest request)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("Пользователь не найден"));
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Patronymic = request.Patronymic;
        user.Phone = request.Phone;
        user.UpdatedAt = _dateTimeProvider.UtcNow;

        await _repository.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result<UserDto>> GetByIdAsync(int userId)
    {
        var user = await _repository.GetByIdWithPharmacyAsync(userId);
        if (user is null)
        {
            return Result.Failure<UserDto>(Error.NotFound("Пользователь не найден"));
        }

        PharmacyDto? pharmacyDto = null;
        if (user.Role == UserRoleEnum.Employee && user.Pharmacy is not null)
        {
            pharmacyDto = new PharmacyDto(
                user.Pharmacy.Id,
                user.Pharmacy.Name,
                user.Pharmacy.Phone,
                new AddressDto(
                    user.Pharmacy.Address.Id,
                    user.Pharmacy.Address.OsmId,
                    user.Pharmacy.Address.Region,
                    user.Pharmacy.Address.State,
                    user.Pharmacy.Address.City,
                    user.Pharmacy.Address.Suburb,
                    user.Pharmacy.Address.Street,
                    user.Pharmacy.Address.HouseNumber,
                    user.Pharmacy.Address.Postcode,
                    user.Pharmacy.Address.Latitude,
                    user.Pharmacy.Address.Longitude
                ));
        }

        return Result.Success(new UserDto(user.Id, user.Role, user.Email, user.PasswordHash, user.EmailVerified,
            user.FirstName, user.LastName, user.Patronymic, user.Phone, pharmacyDto));
    }

    public async Task<Result<UserDto>> GetByEmailAsync(string email)
    {
        var user = await _repository.GetByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure<UserDto>(Error.NotFound("Пользователь не найден"));
        }
        
        return Result.Success(new UserDto(user.Id, user.Role, user.Email, user.PasswordHash, user.EmailVerified, user.FirstName,
            user.LastName, user.Patronymic, user.Phone, null));
    }
    
    public async Task<Result> UpdateEmailRequestAsync(int userId, string newEmail)
    {
        var user = await _repository.GetByEmailAsync(newEmail);
        if (user is not null)
        {
            return Result.Failure(Error.Conflict("Пользователь с таким email уже зарегистрирован"));
        }

        var sendResult = await _emailVerificationService.SendCodeAsync(userId, newEmail, false, VerificationPurposeEnum.EmailChange);
        if (sendResult.IsFailure)
        {
            return Result.Failure<string>(sendResult.Error);
        }
        
        return Result.Success();
    }
    
    public async Task<Result> UpdatePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("Пользователь не найден"));
        }

        if (!_passwordProvider.Verify(currentPassword, user.PasswordHash))
        {
            return Result.Failure(Error.Failure("Неверный текущий пароль"));
        }
        
        if (_passwordProvider.Verify(newPassword, user.PasswordHash))
        {
            return Result.Failure(Error.Failure("Новый пароль совпадает с текущим"));
        }

        user.PasswordHash = _passwordProvider.Hash(newPassword);
        user.UpdatedAt = _dateTimeProvider.UtcNow;
        await _repository.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(int userId, UpdateUserDto dto)
    {
        var user = await _repository.GetByIdAsync(userId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("Пользователь не найден"));
        }

        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _repository.GetByEmailAsync(dto.Email, userId);
            if (existing is not null)
            {
                return Result.Failure(Error.Conflict("Пользователь с таким email уже зарегистрирован"));
            }
        }

        if (dto.Role == UserRoleEnum.Employee && dto.PharmacyId is not null)
        {
            var exists = await _pharmacyRepository.ExistsByIdAsync(dto.PharmacyId.Value);
            if (!exists)
            {
                return Result.Failure(Error.NotFound("Указанная аптека не найдена"));
            }
        }

        user.Email = dto.Email;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Patronymic = dto.Patronymic;
        user.Phone = dto.Phone;
        user.Role = dto.Role;
        user.PharmacyId = dto.Role == UserRoleEnum.Employee ? dto.PharmacyId : null;
        user.UpdatedAt = _dateTimeProvider.UtcNow;

        await _repository.UpdateAsync(user);
        return Result.Success();
    }
    
    public async Task<Result<PaginatedList<UserDto>>> GetPaginatedUsersAsync(UserFilters filters, int pageNumber, int pageSize)
    {
        var query = _repository.Query();

        if (!string.IsNullOrWhiteSpace(filters.FirstName))
        {
            query = query.Where(u => u.FirstName.Contains(filters.FirstName));
        }

        if (!string.IsNullOrWhiteSpace(filters.LastName))
        {
            query = query.Where(u => u.LastName.Contains(filters.LastName));
        }

        if (!string.IsNullOrWhiteSpace(filters.Patronymic))
        {
            query = query.Where(u => u.Patronymic.Contains(filters.Patronymic));
        }

        if (!string.IsNullOrWhiteSpace(filters.Email))
        {
            query = query.Where(u => u.Email.Contains(filters.Email));
        }

        if (filters.Role.HasValue)
        {
            query = query.Where(u => u.Role == filters.Role.Value);
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto(
                u.Id, 
                u.Role, 
                u.Email, 
                null, 
                u.EmailVerified, 
                u.FirstName, 
                u.LastName,
                u.Patronymic, 
                u.Phone,
                null))
            .ToListAsync();

        return Result.Success(new PaginatedList<UserDto>(users, totalCount, pageNumber, pageSize));
    }
}