using Vention.Application.Abstractions;
using Vention.Application.Auth.Contracts;
using Vention.Application.Exceptions;
using Vention.Application.Messaging;
using Vention.Domain.Membership;
using Vention.Domain.Organizations;
using Vention.Domain.Users;

namespace Vention.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IMembershipRepository membershipRepository,
        IOrganizationRepository organizationRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _organizationRepository = organizationRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken ct)
    {
        var email = Email.Create(command.Email);
        var user = await _userRepository.GetByEmailAsync(email, ct);

        if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        var memberships = await _membershipRepository.GetByUserIdAsync(user.Id, ct);
        var membershipDtos = new List<AuthMembershipDto>(memberships.Count);

        foreach (var membership in memberships)
        {
            var organization = await _organizationRepository.GetByIdAsync(membership.OrganizationId, ct);
            if (organization is null)
                continue;

            membershipDtos.Add(new AuthMembershipDto(
                organization.Id.Value,
                organization.Name,
                membership.Role.ToString().ToUpperInvariant()));
        }

        var highestRole = membershipDtos.Count == 0
            ? MembershipRole.Member.ToString().ToUpperInvariant()
            : membershipDtos
                .Select(m => Enum.Parse<MembershipRole>(m.Role, ignoreCase: true))
                .Min()
                .ToString()
                .ToUpperInvariant();

        var (accessToken, accessExpiresAt) = _tokenGenerator.GenerateAccessToken(user);

        return new AuthResponse(
            user.Id.Value,
            user.Email.Value,
            user.Name,
            highestRole,
            accessToken,
            accessExpiresAt,
            membershipDtos);
    }
}