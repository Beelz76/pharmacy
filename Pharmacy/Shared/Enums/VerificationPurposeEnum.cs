using System.ComponentModel;

namespace Pharmacy.Shared.Enums;

public enum VerificationPurposeEnum
{
    [Description("Регистрация")] Registration = 1,
    [Description("Восстановление пароля")] PasswordReset = 2,
    [Description("Смена почты")] EmailChange = 3
}