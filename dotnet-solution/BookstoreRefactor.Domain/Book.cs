namespace BookstoreRefactor.Domain;

public sealed class Book
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public Guid? AuthorId { get; private set; }
    public decimal Price { get; private set; }
    public Guid CategoryId { get; private set; }
    public BookStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Book()
    {
    }

    private Book(string title, string author, Guid categoryId, decimal price, BookStatus status, Guid? authorId = null)
    {
        Id = Guid.NewGuid();
        Title = title;
        Author = author;
        AuthorId = authorId;
        CategoryId = categoryId;
        Price = price;
        Status = status;
        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public static Book Create(string title, string author, Guid categoryId, decimal price, BookStatus status = BookStatus.Draft, Guid? authorId = null)
    {
        EnsureTitle(title);
        EnsureAuthor(author);
        EnsureCategory(categoryId);
        EnsurePrice(price);
        EnsureStatus(status);

        return new Book(title.Trim(), author.Trim(), categoryId, price, status, authorId);
    }

    public void UpdateTitle(string title)
    {
        EnsureTitle(title);
        Title = title.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAuthor(string author)
    {
        EnsureAuthor(author);
        Author = author.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAuthorId(Guid authorId)
    {
        if (authorId == Guid.Empty)
        {
            throw new DomainException("Author identifier is required.");
        }

        AuthorId = authorId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCategory(Guid categoryId)
    {
        EnsureCategory(categoryId);
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal price)
    {
        EnsurePrice(price);
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        ChangeStatus(BookStatus.Active);
    }

    public void Deactivate()
    {
        ChangeStatus(BookStatus.Inactive);
    }

    public void ChangeStatus(BookStatus nextStatus)
    {
        EnsureStatus(nextStatus);

        if (!IsValidTransition(Status, nextStatus))
        {
            throw new DomainException($"Invalid status transition from {Status} to {nextStatus}.");
        }

        Status = nextStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    private static bool IsValidTransition(BookStatus current, BookStatus next)
    {
        return (current, next) switch
        {
            (BookStatus.Draft, BookStatus.Active) => true,
            (BookStatus.Draft, BookStatus.Inactive) => true,
            (BookStatus.Draft, BookStatus.Archived) => true,
            (BookStatus.Active, BookStatus.Inactive) => true,
            (BookStatus.Active, BookStatus.OutOfStock) => true,
            (BookStatus.Active, BookStatus.Archived) => true,
            (BookStatus.Inactive, BookStatus.Active) => true,
            (BookStatus.Inactive, BookStatus.Archived) => true,
            (BookStatus.OutOfStock, BookStatus.Active) => true,
            (BookStatus.OutOfStock, BookStatus.Archived) => true,
            (BookStatus.Archived, BookStatus.Archived) => true,
            _ => false
        };
    }

    private static void EnsureTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Book title is required.");
        }
    }

    private static void EnsureAuthor(string author)
    {
        if (string.IsNullOrWhiteSpace(author))
        {
            throw new DomainException("Book author is required.");
        }
    }

    private static void EnsureCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            throw new DomainException("Category identifier is required.");
        }
    }

    private static void EnsurePrice(decimal price)
    {
        if (price < 0m)
        {
            throw new DomainException("Book price must be greater than or equal to zero.");
        }
    }

    private static void EnsureStatus(BookStatus status)
    {
        if (!Enum.IsDefined(typeof(BookStatus), status))
        {
            throw new DomainException("Book status is invalid.");
        }
    }
}
