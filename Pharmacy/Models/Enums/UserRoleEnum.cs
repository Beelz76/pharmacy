using System.ComponentModel;

namespace Pharmacy.Models.Enums;

public enum UserRoleEnum
{
    [Description("Пользователь")] User = 1,
    [Description("Работник")] Customer = 2,
    [Description("Администратор")] Admin = 3,
}