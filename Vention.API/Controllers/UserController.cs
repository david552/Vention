using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vention.Application.User.Requests;
using Vention.Application.User.Services;

namespace Vention.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpPost]
        public IActionResult Create(UserRequestCreateModel model)
        {
            if (model == null)
            {
                return BadRequest("Data is empty!");
            }

            var createdUser = _userService.Create(model);

            return Ok(createdUser);
        }
    }
}
