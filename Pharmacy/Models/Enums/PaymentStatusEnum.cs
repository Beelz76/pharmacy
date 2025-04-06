using System.ComponentModel;

namespace Pharmacy.Models.Enums;

public enum PaymentStatusEnum
{
    [Description("Оплачено")] Paid = 1,
    [Description("Не оплачено")] NotPaid = 2
}