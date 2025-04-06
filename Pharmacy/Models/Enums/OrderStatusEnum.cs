using System.ComponentModel;

namespace Pharmacy.Models.Enums;

public enum OrderStatusEnum
{
    [Description("Ожидает обработки")] Pending = 1,
    [Description("В обработке")] Processing = 2,
    [Description("Отправлен")] Shipped = 3,
    [Description("Доставлен")] Delivered = 4,
    [Description("Отменен")] Cancelled = 5
}