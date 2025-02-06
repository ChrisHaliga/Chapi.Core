using Chapi.Api.Middleware;
using Chapi.Api.Models.Groups;
using Chapi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chapi.Api.Controllers
{
    [ApiKeyAuthorization]
    [ApiController]
    [Route("[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly GroupsService _groupsService;
        public GroupsController(GroupsService groupsService)
        {
            _groupsService = groupsService;
        }

        [HttpGet("{name}")]
        public async Task<GroupDto?> Get([FromRoute] string name, CancellationToken cancellationToken)
        {
            return await _groupsService.GetAsync(name, cancellationToken);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Group group, CancellationToken cancellationToken)
        {

            await _groupsService.CreateAsync(group, cancellationToken);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Group group, CancellationToken cancellationToken)
        {
            await _groupsService.UpdateAsync(group, cancellationToken);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] Group group, CancellationToken cancellationToken)
        {
            await _groupsService.DeleteAsync(group, cancellationToken);
            return Ok();
        }
    }
}
