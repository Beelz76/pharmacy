using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Pharmacy.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatusEnum
{
    [Description("В ожидании")] Pending = 1,
    [Description("Завершено")] Completed = 2,
    [Description("Ошибка")] Failed = 3,
    [Description("Отменено")] Cancelled = 4,
    [Description("Не оплачено")] NotPaid  = 5,
    [Description("Возвращено")] Refunded  = 6,
}