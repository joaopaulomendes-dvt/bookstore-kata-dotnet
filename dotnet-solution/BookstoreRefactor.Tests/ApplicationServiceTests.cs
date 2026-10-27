using BookstoreRefactor.Application;
using BookstoreRefactor.Domain;
using Moq;
using Xunit;

namespace BookstoreRefactor.Tests;

public class ApplicationServiceTests
{
    [Fact]
    public async Task CreateBook_WhenCategoryExists_ReturnsResponse()
    {
        var category = Category.Create("Programming", "Tech books");
        var categoryRepo = new Mock<ICategoryRepository>();
        categoryRepo.Setup(x => x.ExistsAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var bookRepo = new Mock<IBookRepository>();
        bookRepo.Setup(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var service = new BookApplicationService(bookRepo.Object, categoryRepo.Object);

        var result = await service.CreateAsync(new BookCreateRequest(
            "Clean Architecture",
            "Robert Martin",
            category.Id,
            59.90m,
            BookStatus.Draft));

        Assert.Equal("Clean Architecture", result.Title);
        Assert.Equal(category.Id, result.CategoryId);
        Assert.Equal(59.90m, result.Price);
    }

    [Fact]
    public async Task CreateBook_WhenCategoryDoesNotExist_ThrowsNotFoundException()
    {
        var categoryRepo = new Mock<ICategoryRepository>();
        categoryRepo.Setup(x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var bookRepo = new Mock<IBookRepository>();
        var service = new BookApplicationService(bookRepo.Object, categoryRepo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(new BookCreateRequest(
            "Refactoring",
            "Martin Fowler",
            Guid.NewGuid(),
            49.99m,
            BookStatus.Draft)));
    }

    [Fact]
    public async Task UpdateBook_WhenPriceIsNegative_ThrowsValidationException()
    {
        var categoryId = Guid.NewGuid();
        var book = Book.Create("Domain Modeling", "Eric Evans", categoryId, 40m, BookStatus.Draft);

        var bookRepo = new Mock<IBookRepository>();
        bookRepo.Setup(x => x.GetByIdAsync(book.Id, It.IsAny<CancellationToken>())).ReturnsAsync(book);

        var categoryRepo = new Mock<ICategoryRepository>();
        categoryRepo.Setup(x => x.ExistsAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var service = new BookApplicationService(bookRepo.Object, categoryRepo.Object);

        await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(new BookUpdateRequest(
            book.Id,
            null,
            null,
            null,
            -10m,
            null)));
    }

    [Fact]
    public async Task GetById_WhenBookDoesNotExist_ThrowsNotFoundException()
    {
        var bookRepo = new Mock<IBookRepository>();
        bookRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Book?)null);
        var categoryRepo = new Mock<ICategoryRepository>();

        var service = new BookApplicationService(bookRepo.Object, categoryRepo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateCategory_WhenValid_ReturnsResponse()
    {
        var categoryRepo = new Mock<ICategoryRepository>();
        categoryRepo.Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var service = new CategoryApplicationService(categoryRepo.Object);

        var result = await service.CreateAsync(new CategoryCreateRequest("Fiction", "Story books"));

        Assert.Equal("Fiction", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task ToggleCategoryStatus_WhenInactive_ActivatesCategory()
    {
        var category = Category.Create("Children", "Kids books");
        category.Deactivate();

        var categoryRepo = new Mock<ICategoryRepository>();
        categoryRepo.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        categoryRepo.Setup(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var service = new CategoryApplicationService(categoryRepo.Object);

        var result = await service.ToggleStatusAsync(category.Id);

        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateAuthor_WhenValid_ReturnsResponse()
    {
        var authorRepo = new Mock<IAuthorRepository>();
        authorRepo.Setup(x => x.AddAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var service = new AuthorApplicationService(authorRepo.Object);

        var result = await service.CreateAsync(new AuthorCreateRequest(
            "George Orwell",
            new DateTime(1903, 6, 25),
            "British",
            "george@example.com"));

        Assert.Equal("George Orwell", result.Name);
        Assert.True(result.IsActive);
        Assert.Equal("British", result.Nationality);
    }

    [Fact]
    public async Task DeleteAuthor_WhenAuthorExists_SoftDeletesAuthor()
    {
        var author = Author.Create("Jane Austen", new DateTime(1775, 12, 16), "English", "jane@example.com");

        var authorRepo = new Mock<IAuthorRepository>();
        authorRepo.Setup(x => x.GetByIdAsync(author.Id, It.IsAny<CancellationToken>())).ReturnsAsync(author);
        authorRepo.Setup(x => x.UpdateAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var service = new AuthorApplicationService(authorRepo.Object);

        var result = await service.DeleteAsync(author.Id);

        Assert.False(result.IsActive);
    }
}
