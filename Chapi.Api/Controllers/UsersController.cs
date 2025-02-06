using Chapi.Api.Middleware;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models;
using Chapi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chapi.Api.Controllers
{
    public class UsersController : CrudController<User, UserWithId>
    {
        public UsersController(CrudConfigData<User> config, IDatabaseService cosmosService) : base(cosmosService, config) { }


        [HttpGet("{email}")]
        public async Task<IActionResult> Get([FromRoute] string email, CancellationToken cancellationToken)
        {
            var userQuery = new User() { Email = email };
            
            var userFound = await GetItem(userQuery, cancellationToken);

            return userFound == null ? NotFound() : Ok(userFound);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User user, CancellationToken cancellationToken)
        {
            await CreateItem(user, cancellationToken);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] User user, CancellationToken cancellationToken)
        {
            await UpdateItem(user, true, cancellationToken);
            return Ok();
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody] User user, CancellationToken cancellationToken)
        {
            await UpdateItem(user, false, cancellationToken);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] UserMinimalDto userMinimal, CancellationToken cancellationToken)
        {
            await DeleteItem(userMinimal.ToUser(), cancellationToken);
            return Ok();
        }
    }
}
