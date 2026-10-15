using BookstoreRefactor.Domain;
using Xunit;

namespace BookstoreRefactor.Tests;

public class BookDomainTests
{
    [Fact(DisplayName = "Book_Create_ValidData_StoresValues")]
    public void Book_Create_ValidData_StoresValues()
    {
        var categoryId = Guid.NewGuid();
        var book = Book.Create(
            "Clean Architecture",
            "Robert Martin",
            categoryId,
            79.90m,
            BookStatus.Active);

        Assert.Equal("Clean Architecture", book.Title);
        Assert.Equal("Robert Martin", book.Author);
        Assert.Equal(categoryId, book.CategoryId);
        Assert.Equal(79.90m, book.Price);
        Assert.Equal(BookStatus.Active, book.Status);
        Assert.NotEqual(Guid.Empty, book.Id);
    }

    [Theory(DisplayName = "Book_Create_InvalidTitle_ThrowsDomainException")]
    [InlineData("")]
    [InlineData("   ")]
    public void Book_Create_InvalidTitle_ThrowsDomainException(string title)
    {
        var exception = Assert.Throws<DomainException>(() =>
            Book.Create(title, "Robert Martin", Guid.NewGuid(), 29.99m, BookStatus.Draft));

        Assert.Contains("title", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Book_UpdatePrice_NegativePrice_ThrowsDomainException")]
    public void Book_UpdatePrice_NegativePrice_ThrowsDomainException()
    {
        var book = Book.Create("Domain-Driven Design", "Eric Evans", Guid.NewGuid(), 39.99m, BookStatus.Draft);

        var exception = Assert.Throws<DomainException>(() => book.UpdatePrice(-1m));

        Assert.Contains("price", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Book_ChangeStatus_ValidTransition_UpdatesStatus")]
    public void Book_ChangeStatus_ValidTransition_UpdatesStatus()
    {
        var book = Book.Create("The Pragmatic Programmer", "Andy Hunt", Guid.NewGuid(), 45.00m, BookStatus.Draft);

        book.ChangeStatus(BookStatus.Active);

        Assert.Equal(BookStatus.Active, book.Status);
        Assert.NotEqual(default, book.UpdatedAt);
    }

    [Fact(DisplayName = "Book_ChangeStatus_InvalidTransition_ThrowsDomainException")]
    public void Book_ChangeStatus_InvalidTransition_ThrowsDomainException()
    {
        var book = Book.Create("Refactoring", "Martin Fowler", Guid.NewGuid(), 53.50m, BookStatus.Archived);

        var exception = Assert.Throws<DomainException>(() => book.ChangeStatus(BookStatus.Active));

        Assert.Contains("transition", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Category_Create_ValidValues_StoresState")]
    public void Category_Create_ValidValues_StoresState()
    {
        var category = Category.Create("Programming", "Software engineering books");

        Assert.Equal("Programming", category.Name);
        Assert.Equal("Software engineering books", category.Description);
        Assert.True(category.IsActive);
    }

    [Fact(DisplayName = "Category_Deactivate_UpdatesInactiveState")]
    public void Category_Deactivate_UpdatesInactiveState()
    {
        var category = Category.Create("Fiction", "Story-driven books");

        category.Deactivate();

        Assert.False(category.IsActive);
    }

    [Fact(DisplayName = "Category_Activate_WhenInactive_Reactivates")]
    public void Category_Activate_WhenInactive_Reactivates()
    {
        var category = Category.Create("Children", "Books for kids");
        category.Deactivate();

        category.Activate();

        Assert.True(category.IsActive);
    }

    [Fact(DisplayName = "Category_Create_InvalidName_ThrowsDomainException")]
    public void Category_Create_InvalidName_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() => Category.Create("   ", "desc"));

        Assert.Contains("name", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
