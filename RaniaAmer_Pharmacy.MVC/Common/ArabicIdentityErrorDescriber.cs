using Microsoft.AspNetCore.Identity;

namespace RaniaAmer_Pharmacy.MVC.Common;

// ASP.NET Core Identity's built-in validation messages (password rules, duplicate email, etc.)
// are in English by default. This overrides the common ones in Arabic so error messages
// match the rest of the app, which is fully Arabic.
public class ArabicIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError PasswordTooShort(int length) => new()
    {
        Code = nameof(PasswordTooShort),
        Description = $"كلمة المرور يجب أن تكون {length} أحرف على الأقل"
    };

    public override IdentityError PasswordRequiresDigit() => new()
    {
        Code = nameof(PasswordRequiresDigit),
        Description = "كلمة المرور يجب أن تحتوي على رقم واحد على الأقل"
    };

    public override IdentityError PasswordRequiresUpper() => new()
    {
        Code = nameof(PasswordRequiresUpper),
        Description = "كلمة المرور يجب أن تحتوي على حرف كبير (A-Z) على الأقل"
    };

    public override IdentityError PasswordRequiresLower() => new()
    {
        Code = nameof(PasswordRequiresLower),
        Description = "كلمة المرور يجب أن تحتوي على حرف صغير (a-z) على الأقل"
    };

    public override IdentityError PasswordRequiresNonAlphanumeric() => new()
    {
        Code = nameof(PasswordRequiresNonAlphanumeric),
        Description = "كلمة المرور يجب أن تحتوي على رمز خاص واحد على الأقل"
    };

    public override IdentityError PasswordMismatch() => new()
    {
        Code = nameof(PasswordMismatch),
        Description = "كلمة المرور الحالية غير صحيحة"
    };

    public override IdentityError DuplicateEmail(string email) => new()
    {
        Code = nameof(DuplicateEmail),
        Description = "هذا البريد الإلكتروني مستخدم بالفعل"
    };

    public override IdentityError InvalidEmail(string? email) => new()
    {
        Code = nameof(InvalidEmail),
        Description = "البريد الإلكتروني غير صحيح"
    };

    public override IdentityError DuplicateUserName(string userName) => new()
    {
        Code = nameof(DuplicateUserName),
        Description = "اسم المستخدم مستخدم بالفعل"
    };

    public override IdentityError UserLockoutNotEnabled() => new()
    {
        Code = nameof(UserLockoutNotEnabled),
        Description = "تم قفل الحساب مؤقتًا بسبب محاولات دخول خاطئة متكررة، حاول مرة أخرى بعد قليل"
    };

    public override IdentityError InvalidToken() => new()
    {
        Code = nameof(InvalidToken),
        Description = "الرابط أو الرمز غير صالح أو منتهي الصلاحية"
    };
}
