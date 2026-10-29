using System.Net.Mail;
using Bookstore.Domain.Common;

namespace Bookstore.Domain.Authors;

public sealed record Author
{
    private Author(
        string id,
        string name,
        string normalizedName,
        string? biography,
        string? website,
        DateOnly? birthDate,
        string? nationality,
        string? email,
        bool active,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        Name = name;
        NormalizedName = normalizedName;
        Biography = biography;
        Website = website;
        BirthDate = birthDate;
        Nationality = nationality;
        Email = email;
        Active = active;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public string Id { get; init; }

    public string Name { get; init; }

    public string NormalizedName { get; init; }

    public string? Biography { get; init; }

    public string? Website { get; init; }

    public DateOnly? BirthDate { get; init; }

    public string? Nationality { get; init; }

    public string? Email { get; init; }

    public bool Active { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public static Author Create(
        string? name,
        string? biography,
        string? website,
        DateOnly? birthDate,
        string? nationality,
        string? email,
        DateTimeOffset now)
    {
        var trimmedName = name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            throw new DomainValidationException("Name is required.");
        }

        if (trimmedName.Length > 255)
        {
            throw new DomainValidationException("Name must not exceed 255 characters.");
        }

        var normalizedWebsite = NormalizeOptional(website);
        if (normalizedWebsite is not null &&
            (!Uri.TryCreate(normalizedWebsite, UriKind.Absolute, out var websiteUri) ||
             websiteUri.Scheme is not ("http" or "https")))
        {
            throw new DomainValidationException("Website must be an absolute HTTP or HTTPS URL.");
        }

        var normalizedEmail = NormalizeOptional(email);
        if (normalizedEmail is not null && !MailAddress.TryCreate(normalizedEmail, out _))
        {
            throw new DomainValidationException("Email must be a valid email address.");
        }

        return new Author(
            string.Empty,
            trimmedName,
            NormalizeName(trimmedName),
            NormalizeOptional(biography),
            normalizedWebsite,
            birthDate,
            NormalizeOptional(nationality),
            normalizedEmail,
            true,
            now.ToUniversalTime(),
            now.ToUniversalTime());
    }

    public static Author Restore(
        string id,
        string name,
        string normalizedName,
        string? biography,
        string? website,
        DateOnly? birthDate,
        string? nationality,
        string? email,
        bool active,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new Author(
            id,
            name,
            normalizedName,
            biography,
            website,
            birthDate,
            nationality,
            email,
            active,
            createdAt.ToUniversalTime(),
            updatedAt.ToUniversalTime());
    }

    public Author WithId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new DomainValidationException("ID is required.");
        }

        return this with { Id = id };
    }

    public Author Deactivate(DateTimeOffset now)
    {
        if (!Active)
        {
            return this;
        }

        return this with { Active = false, UpdatedAt = now.ToUniversalTime() };
    }

    public static string NormalizeName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return string.Join(' ', name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();
    }

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
