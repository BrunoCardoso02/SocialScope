using System.Text.RegularExpressions;
using SocialScope.Domain.Exceptions;

namespace SocialScope.Domain.ValueObjects;

public sealed partial record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !EmailRegex().IsMatch(value))
        {
            throw new InvalidEmailException(value);
        }

        return new Email(value);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
