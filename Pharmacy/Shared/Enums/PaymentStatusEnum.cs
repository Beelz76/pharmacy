using System.ComponentModel;

namespace Pharmacy.Shared.Enums;

public enum PaymentStatusEnum
{
    [Description("В ожидании")] Pending = 1,
    [Description("Завершено")] Completed = 2,
    [Description("Ошибка")] Failed = 3,
    [Description("Отменено")] Cancelled = 4
}