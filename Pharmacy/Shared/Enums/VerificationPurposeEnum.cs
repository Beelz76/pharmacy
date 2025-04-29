using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Pharmacy.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VerificationPurposeEnum
{
    [Description("Регистрация")] Registration = 1,
    [Description("Восстановление пароля")] PasswordReset = 2,
    [Description("Смена почты")] EmailChange = 3
}