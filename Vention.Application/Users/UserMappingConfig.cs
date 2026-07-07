using Mapster;
using Vention.Application.Users.Contracts;
using Vention.Domain.Users;

namespace Vention.Application.Users
{
    public sealed class UserMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<User, UserResponse>()
                .Map(dest => dest.Id, src => src.Id.Value)
                .Map(dest => dest.Email, src => src.Email.Value);
        }
    }
}
