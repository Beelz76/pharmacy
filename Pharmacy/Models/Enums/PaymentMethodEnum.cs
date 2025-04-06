using System.ComponentModel;

namespace Pharmacy.Models.Enums;

public enum PaymentMethodEnum
{
    [Description("Онлайн")] Online = 1,
    [Description("Карта")] Card = 2,
    [Description("Наличные")] Cash = 3
}