namespace Pharmacy.Extensions;

public static class EntityChangesExtensions
{
    public static bool HasChanges<TModel, TUpdate>(TModel model, TUpdate update, params Func<TModel, TUpdate, bool>[] comparisons)
    {
        return comparisons.Any(compare => compare(model, update));
    }
}