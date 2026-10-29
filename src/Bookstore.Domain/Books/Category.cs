namespace Bookstore.Domain.Books;

public sealed record Category(int Id, string Name);

public static class Categories
{
    public static IReadOnlyList<Category> All { get; } =
    [
        new(1, "Fiction"),
        new(2, "Non-Fiction"),
        new(3, "Science"),
        new(4, "Biography"),
        new(5, "Children"),
        new(6, "Programming")
    ];

    public static int? FindId(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return All.FirstOrDefault(category =>
            string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase))?.Id;
    }
}
