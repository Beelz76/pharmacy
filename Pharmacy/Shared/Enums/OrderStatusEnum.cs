using System.ComponentModel;

namespace Pharmacy.Shared.Enums;

public enum OrderStatusEnum
{
    [Description("Ожидает обработки")] Pending = 1,
    [Description("В обработке")] Processing = 2,
    [Description("Готов к получению")] ReadyForReceive = 3,
    [Description("Отправлен")] Shipped = 4,
    [Description("Доставлен")] Delivered = 5,
    [Description("Отменен")] Cancelled = 6
}