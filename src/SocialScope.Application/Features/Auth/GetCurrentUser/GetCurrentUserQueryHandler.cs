using SocialScope.Application.Exceptions;
using SocialScope.Application.Interfaces;

namespace SocialScope.Application.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetCurrentUserResponse> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(query.UserId, cancellationToken)
            ?? throw new UserNotFoundException();

        return new GetCurrentUserResponse(
            user.Id,
            user.Email.Value,
            user.Name,
            user.CreatedAt,
            user.LastLoginAt);
    }
}
