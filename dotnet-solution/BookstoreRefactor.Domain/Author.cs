namespace BookstoreRefactor.Domain;

public sealed class Author
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime? BirthDate { get; private set; }
    public string Nationality { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Author()
    {
    }

    private Author(string name, DateTime? birthDate, string? nationality, string? email, bool isActive)
    {
        Id = Guid.NewGuid();
        Name = name;
        BirthDate = birthDate;
        Nationality = nationality ?? string.Empty;
        Email = email ?? string.Empty;
        IsActive = isActive;

        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public static Author Create(string name, DateTime? birthDate = null, string? nationality = null, string? email = null, bool isActive = true)
    {
        EnsureName(name);
        EnsureEmail(email);

        return new Author(name.Trim(), birthDate, nationality?.Trim(), email?.Trim(), isActive);
    }

    public void Update(string name, DateTime? birthDate = null, string? nationality = null, string? email = null)
    {
        EnsureName(name);
        EnsureEmail(email);

        Name = name.Trim();
        BirthDate = birthDate;
        Nationality = nationality?.Trim() ?? string.Empty;
        Email = email?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void EnsureName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Author name is required.");
        }
    }

    private static void EnsureEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        if (!email.Contains('@'))
        {
            throw new DomainException("Author email must be a valid email address.");
        }
    }
}
