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

    public string? SecurityQuestion { get; private set; }

    public string? SecurityAnswerHash { get; private set; }

    public bool HasAccount => !string.IsNullOrEmpty(PasswordHash);

    public bool HasSecurityQuestion => !string.IsNullOrEmpty(SecurityQuestion) && !string.IsNullOrEmpty(SecurityAnswerHash);

    // Paused automatically after a long stretch of no orders/no login (see ICustomerService
    // inactivity cleanup). An admin can reactivate manually at any time.
    public bool IsActive { get; private set; } = true;

    // Last time this customer placed an order or logged in. Null means "never" (e.g. a guest
    // record created from a single checkout who never logged in again) — CreatedAt is then used
    // as the baseline for inactivity instead.
    public DateTime? LastActivityAt { get; private set; }

    public DateTime? DeactivatedAt { get; private set; }

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

    public void SetSecurityQuestion(string question, string answerHash)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new BusinessException("سؤال الأمان مطلوب.", nameof(question));

        if (question.Length > 200)
            throw new BusinessException("سؤال الأمان لا يجب أن يتجاوز 200 حرف.", nameof(question));

        if (string.IsNullOrWhiteSpace(answerHash))
            throw new BusinessException("إجابة سؤال الأمان مطلوبة.", nameof(answerHash));

        SecurityQuestion = question.Trim();
        SecurityAnswerHash = answerHash;
        UpdatedAt = DateTime.UtcNow;
    }

    // Called whenever the customer places an order or logs in — resets the inactivity clock.
    public void RecordActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        DeactivatedAt = null;
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
}
