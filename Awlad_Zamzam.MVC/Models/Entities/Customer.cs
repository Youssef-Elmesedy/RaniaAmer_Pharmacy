using Awlad_Zamzam.MVC.Models.Exceptions;

namespace Awlad_Zamzam.MVC.Models.Entities;

public class Customer : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;

    // Null until the customer registers a login for themselves (guest orders don't require one)
    public string? PasswordHash { get; private set; }

    public bool HasAccount => !string.IsNullOrEmpty(PasswordHash);

    // Optional security question and answer for password recovery
    public string? SecurityQuestion { get; private set; }

    public string? SecurityAnswerHash { get; private set; }

    public DateTime? SecurityAnswerUpdatedAt { get; private set; }

    private Customer() { }

    public static Customer Create(string name, string phoneNumber, string address)
    {
        Validate(name, phoneNumber, address);

        return new Customer
        {
            Name = name.Trim(),
            NormalizedName = name.ToUpperInvariant().Trim(),
            PhoneNumber = phoneNumber.Trim(),
            Address = address.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string phoneNumber, string address)
    {
        Validate(name, phoneNumber, address);

        Name = name.Trim();

        NormalizedName = name.ToUpperInvariant().Trim();

        PhoneNumber = phoneNumber.Trim();

        Address = address.Trim();

        UpdatedAt = DateTime.Now;
    }

    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new BusinessException("Password hash cannot be empty.", nameof(passwordHash));

        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(string name, string phoneNumber, string address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("Name cannot be null or empty.", nameof(name));

        if (name.Length > 50)
            throw new BusinessException("Name cannot exceed 50 characters.", nameof(name));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new BusinessException("Phone number cannot be null or empty.", nameof(phoneNumber));

        if (phoneNumber.Length != 11)
            throw new BusinessException("Phone number must be 11 digits.", nameof(phoneNumber));

        if (string.IsNullOrWhiteSpace(address))
            throw new BusinessException("Address cannot be null or empty.", nameof(address));
    }

    public void SetSecurityQuestion(string question, string answerHash)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new BusinessException("يجب اختيار سؤال الأمان.", nameof(question));

        if (string.IsNullOrWhiteSpace(answerHash))
            throw new BusinessException("إجابة سؤال الأمان غير صحيحة.", nameof(answerHash));

        SecurityQuestion = question.Trim();
        SecurityAnswerHash = answerHash;
        SecurityAnswerUpdatedAt = DateTime.UtcNow;

        UpdatedAt = DateTime.UtcNow;
    }
}
