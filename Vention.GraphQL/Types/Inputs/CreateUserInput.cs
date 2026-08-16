namespace Vention.GraphQL.Types.Inputs
{

    public sealed class CreateUserInput
    {
        public string Email { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}