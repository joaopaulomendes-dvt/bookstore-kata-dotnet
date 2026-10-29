using Bookstore.Domain.Authors;
using Bookstore.Domain.Common;

namespace Bookstore.Tests.Unit;

public sealed class AuthorDomainTests
{
    private static readonly DateTimeOffset Now = new(2025, 1, 2, 3, 4, 5, TimeSpan.Zero);

    [Fact]
    public void Create_ValidProfile_TrimsNameAndSetsDefaults()
    {
        var author = Author.Create(
            "  Jane Austen ",
            "English novelist",
            "https://example.test",
            new DateOnly(1775, 12, 16),
            "British",
            "jane@example.test",
            Now);

        Assert.Equal("Jane Austen", author.Name);
        Assert.Equal("JANE AUSTEN", author.NormalizedName);
        Assert.True(author.Active);
        Assert.Equal(Now, author.CreatedAt);
        Assert.Equal(Now, author.UpdatedAt);
        Assert.Equal(string.Empty, author.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_ThrowsDomainValidationException(string name)
    {
        Assert.Throws<DomainValidationException>(() =>
            Author.Create(name, null, null, null, null, null, Now));
    }

    [Fact]
    public void Create_NameLongerThan255Characters_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() =>
            Author.Create(new string('a', 256), null, null, null, null, null, Now));
    }

    [Fact]
    public void Create_InvalidWebsite_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() =>
            Author.Create("Jane Austen", null, "file:///tmp/profile", null, null, null, Now));
    }

    [Fact]
    public void Create_InvalidEmail_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() =>
            Author.Create("Jane Austen", null, null, null, null, "not-an-email", Now));
    }

    [Fact]
    public void Deactivate_ActiveAuthor_ReturnsInactiveCopyWithUpdatedTimestamp()
    {
        var author = Author.Create("Jane Austen", null, null, null, null, null, Now);
        var deletedAt = Now.AddDays(1);

        var inactive = author.Deactivate(deletedAt);

        Assert.True(author.Active);
        Assert.False(inactive.Active);
        Assert.Equal(Now, inactive.CreatedAt);
        Assert.Equal(deletedAt, inactive.UpdatedAt);
    }

    [Fact]
    public void NormalizeName_CaseAndWhitespaceDifferences_ReturnSameValue()
    {
        Assert.Equal(Author.NormalizeName("  Jane  Austen "), Author.NormalizeName("jane austen"));
    }
}
