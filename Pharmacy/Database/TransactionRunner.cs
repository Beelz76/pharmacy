using Pharmacy.Shared.Result;

namespace Pharmacy.Database;

public class TransactionRunner
{
    private readonly PharmacyDbContext _context;

    public TransactionRunner(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<Result> ExecuteAsync(Func<Task<Result>> action)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await action();
            
            if (result.IsSuccess)
                await transaction.CommitAsync();
            else
                await transaction.RollbackAsync();
            
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Result.Failure(Error.Problem("Не удалось выполнить операцию"));
        }
    }

    public async Task<Result<T>> ExecuteAsync<T>(Func<Task<Result<T>>> action)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await action();
            
            if (result.IsSuccess)
                await transaction.CommitAsync();
            else
                await transaction.RollbackAsync();
            
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Result.Failure<T>(Error.Problem("Не удалось выполнить операцию"));
        }
    }
}