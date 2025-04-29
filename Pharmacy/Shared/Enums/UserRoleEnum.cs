using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Pharmacy.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRoleEnum
{
    [Description("Пользователь")] User = 1,
    [Description("Сотрудник")] Employee = 2,
    [Description("Администратор")] Admin = 3,
}