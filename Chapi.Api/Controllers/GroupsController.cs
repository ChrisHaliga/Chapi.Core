using Chapi.Api.Middleware;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Chapi.Api.Services.CrudServices;
using Chapi.Api.Models;

namespace Chapi.Api.Controllers
{
    [ApiKeyAuthorization]
    [ApiController]
    [Route("[controller]")]
    public class GroupsController(GroupService GroupService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? name = null, [FromQuery] string? parent = null, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return Ok(await GroupService.GetGroupByName(name, cancellationToken));
            }

            var groupsFound = new List<GroupWithId>();

            if (!string.IsNullOrEmpty(parent))
            {
                groupsFound = await GroupService.GetGroupsByParent(parent, cancellationToken);
            }
            else
            {
                groupsFound = await GroupService.GetAllItems();
            }

            return groupsFound.Count() == 0 ? NotFound(groupsFound) : Ok(groupsFound);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Group group, CancellationToken cancellationToken)
        {
            await GroupService.CreateItem(group, cancellationToken);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Group group, CancellationToken cancellationToken)
        {
            await GroupService.UpdateItem(group, true, cancellationToken);
            return Ok();
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody] Group group, CancellationToken cancellationToken)
        {
            await GroupService.UpdateItem(group, false, cancellationToken);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] GroupMinimalDto group, CancellationToken cancellationToken)
        {
            await GroupService.DeleteItem(group.ToGroup(), cancellationToken);
            return Ok();
        }
    }
}
