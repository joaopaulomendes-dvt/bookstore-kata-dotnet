namespace BookstoreRefactor.Infrastructure;

public sealed class MongoOptions
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "bookstore";
    public string BooksCollection { get; set; } = "books";
    public string CategoriesCollection { get; set; } = "categories";
}
