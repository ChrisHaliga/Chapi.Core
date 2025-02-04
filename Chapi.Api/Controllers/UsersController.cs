using Chapi.Api.Middleware;
using Chapi.Api.Models.Users;
using Chapi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chapi.Api.Controllers
{
    [ApiKeyAuthorization]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _userService;
        public UsersController(UsersService userService)
        {
            _userService = userService;
        }

        [HttpGet("{email}")]
        public async Task<User?> Get([FromRoute] string email, CancellationToken cancellationToken)
        {
            return await _userService.GetAsync(email, cancellationToken);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserCreateDto userDto, CancellationToken cancellationToken)
        {

            await _userService.CreateAsync(new User(userDto), cancellationToken);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] User user, CancellationToken cancellationToken)
        {
            await _userService.UpdateAsync(user, cancellationToken);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] User user, CancellationToken cancellationToken)
        {
            await _userService.DeleteAsync(user, cancellationToken);
            return Ok();
        }
    }
}
