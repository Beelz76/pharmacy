using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Pharmacy.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatusEnum
{
    [Description("Ожидает оплаты")] WaitingForPayment = 1,
    [Description("Ожидает обработки")] Pending = 2,
    [Description("В обработке")] Processing = 3,
    [Description("Готов к получению")] ReadyForReceive = 4,
    [Description("Получен")] Received = 5,
    [Description("Отменен")] Cancelled = 6,
    [Description("Возврат средств")] Refunded = 7,
    [Description("Передан в доставку")] OutForDelivery = 8,
    [Description("Доставлен")] Delivered = 9,
}