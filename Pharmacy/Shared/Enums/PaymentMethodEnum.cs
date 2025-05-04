using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Pharmacy.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethodEnum
{
    [Description("Картой онлайн")] Online = 1,
    [Description("При получении")] OnDelivery = 2
}