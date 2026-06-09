using System;
using Vention.Application.User.Requests;
using Vention.Application.User.Responses;



namespace Vention.Application.User.Services
{
    public interface IUserService
    {
        UserResponseModel Create(UserRequestCreateModel model);
        IEnumerable<UserResponseModel> GetAll();
    }
}
