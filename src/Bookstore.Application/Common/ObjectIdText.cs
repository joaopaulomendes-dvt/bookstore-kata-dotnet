namespace Bookstore.Application.Common;

public static class ObjectIdText
{
    public static bool IsValid(string? value)
    {
        return value is { Length: 24 } &&
               value.All(character =>
                   character is >= '0' and <= '9' or
                       >= 'a' and <= 'f' or
                       >= 'A' and <= 'F');
    }
}
