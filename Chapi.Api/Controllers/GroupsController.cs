using Chapi.Api.Middleware;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models;
using Chapi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chapi.Api.Controllers
{
    public class GroupsController : CrudController<Group, GroupWithId>
    {
        public GroupsController(CrudConfigData<Group> config, IDatabaseService cosmosService) : base(cosmosService, config) { }


        [HttpGet("{name}")]
        public async Task<Group?> Get([FromRoute] string name, CancellationToken cancellationToken)
        {
            var user = new Group() { Name = name };
            return await GetItem(user, cancellationToken);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Group group, CancellationToken cancellationToken)
        {
            await CreateItem(group, cancellationToken);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Group group, CancellationToken cancellationToken)
        {
            await UpdateItem(group, true, cancellationToken);
            return Ok();
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody] Group group, CancellationToken cancellationToken)
        {
            await UpdateItem(group, false, cancellationToken);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] GroupMinimalDto group, CancellationToken cancellationToken)
        {
            await DeleteItem(group.ToGroup(), cancellationToken);
            return Ok();
        }
    }
}
