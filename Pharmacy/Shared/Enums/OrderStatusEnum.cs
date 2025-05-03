using System.ComponentModel;

namespace Pharmacy.Shared.Enums;

public enum OrderStatusEnum
{
    [Description("Ожидает обработки")] Pending = 1,
    [Description("В обработке")] Processing = 2,
    [Description("Готов к получению")] ReadyForReceive = 3,
    [Description("Получен")] Received = 4,
    [Description("Отменен")] Cancelled = 5,
    [Description("Возврат средств")] Refunded = 6
}