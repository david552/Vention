using System.Text.RegularExpressions;

namespace Vention.Domain.Users
{
    public sealed partial record Email
    {
        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty.", nameof(value));

            var normalized = value.Trim().ToLowerInvariant();

            if (!EmailRegex().IsMatch(normalized))
                throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));

            return new Email(normalized);
        }

        public override string ToString() => Value;

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex EmailRegex();

        
    }
}
