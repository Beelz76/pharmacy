using System.Net;
using Pharmacy.Models.Enums;
using Pharmacy.Models.Result;

namespace Pharmacy.Data.Repositories;

public abstract class BaseRepository
{
    private readonly PharmacyDbContext _context;

    protected BaseRepository(PharmacyDbContext context)
    {
        _context = context;
    }
    
    public async Task<Result> ExecuteInTransactionAsync(Func<Task<Result>> action)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await action();
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Result.Failure(HttpStatusCode.InternalServerError, ErrorTypeEnum.InternalServerError, "Не удалось выполнить операцию");
        }
    }
    
    public async Task<Result<TResult>> ExecuteInTransactionAsync<TResult>(Func<Task<Result<TResult>>> action)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await action();
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Result<TResult>.Failure(HttpStatusCode.InternalServerError, ErrorTypeEnum.InternalServerError, "Не удалось выполнить операцию");
        }
    }
}