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
            Enum.Parse<BookStatus>(document.Status));

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
