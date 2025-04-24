using System.ComponentModel;

namespace Pharmacy.Shared.Enums;

public enum UserRoleEnum
{
    [Description("Пользователь")] User = 1,
    [Description("Сотрудник")] Employee = 2,
    [Description("Администратор")] Admin = 3,
}