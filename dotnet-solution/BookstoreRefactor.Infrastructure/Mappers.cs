using System.Reflection;
using BookstoreRefactor.Domain;

namespace BookstoreRefactor.Infrastructure;

public static class BookMapper
{
    public static Book ToDomain(this BookDocument document)
    {
        var book = Book.Create(
            document.Title,
            document.Author,
            document.CategoryId,
            document.Price,
            Enum.Parse<BookStatus>(document.Status),
            document.AuthorId);

        SetProperty(book, nameof(Book.Id), document.Id);
        SetProperty(book, nameof(Book.CreatedAt), document.CreatedAt);
        SetProperty(book, nameof(Book.UpdatedAt), document.UpdatedAt);

        return book;
    }

    public static BookDocument ToDocument(this Book entity)
        => new()
        {
            Id = entity.Id,
            Title = entity.Title,
            Author = entity.Author,
            AuthorId = entity.AuthorId,
            Price = entity.Price,
            CategoryId = entity.CategoryId,
            Status = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };

    private static void SetProperty(object instance, string propertyName, object value)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property?.SetValue(instance, value);
    }
}

public static class CategoryMapper
{
    public static Category ToDomain(this CategoryDocument document)
    {
        var category = Category.Create(document.Name, document.Description, document.IsActive);

        SetProperty(category, nameof(Category.Id), document.Id);
        SetProperty(category, nameof(Category.CreatedAt), document.CreatedAt);
        SetProperty(category, nameof(Category.UpdatedAt), document.UpdatedAt);

        return category;
    }

    public static CategoryDocument ToDocument(this Category entity)
        => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };

    private static void SetProperty(object instance, string propertyName, object value)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property?.SetValue(instance, value);
    }
}

public static class AuthorMapper
{
    public static Author ToDomain(this AuthorDocument document)
    {
        var author = Author.Create(document.Name, document.BirthDate, document.Nationality, document.Email, document.IsActive);

        SetProperty(author, nameof(Author.Id), document.Id);
        SetProperty(author, nameof(Author.CreatedAt), document.CreatedAt);
        SetProperty(author, nameof(Author.UpdatedAt), document.UpdatedAt);

        return author;
    }

    public static AuthorDocument ToDocument(this Author entity)
        => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            BirthDate = entity.BirthDate,
            Nationality = entity.Nationality,
            Email = entity.Email,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };

    private static void SetProperty(object instance, string propertyName, object value)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property?.SetValue(instance, value);
    }
}
