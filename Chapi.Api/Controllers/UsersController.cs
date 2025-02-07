using Chapi.Api.Middleware;
using Chapi.Api.Models;
using Chapi.Api.Services.CrudServices;
using Microsoft.AspNetCore.Mvc;

namespace Chapi.Api.Controllers
{
    [ApiKeyAuthorization]
    [ApiController]
    [Route("[controller]")]
    public class UsersController(UserService UserService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? email = null, [FromQuery] string? organization = null, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(email))
            {
                return Ok(await UserService.GetUserByEmail(email, cancellationToken));
            }

            var usersFound = new List<UserWithId>();

            if (!string.IsNullOrEmpty(organization))
            {
                usersFound = await UserService.GetUsersByOrganization(organization, cancellationToken);
            }
            else
            {
                usersFound = await UserService.GetAllItems();
            }

            return usersFound.Count() == 0 ? NotFound(usersFound) : Ok(usersFound);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User user, CancellationToken cancellationToken)
        {
            var response = await UserService.CreateItem(user, cancellationToken);

            return RequestDetailObject.RequestStatusToActionResult(response);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] User user, CancellationToken cancellationToken)
        {
            await UserService.UpdateItem(user, true, cancellationToken);
            return Ok();
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody] User user, CancellationToken cancellationToken)
        {
            await UserService.UpdateItem(user, false, cancellationToken);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] UserMinimalDto userMinimal, CancellationToken cancellationToken)
        {
            await UserService.DeleteUser(userMinimal, cancellationToken);
            return Ok();
        }
    }
}
