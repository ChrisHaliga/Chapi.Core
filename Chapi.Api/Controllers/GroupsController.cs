using Chapi.Api.Middleware;
using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services;
using Chapi.Api.Services.CrudServices;
using Microsoft.AspNetCore.Mvc;

namespace Chapi.Api.Controllers
{
    [ApiKeyAuthorization]
    [ApiController]
    [Route("[controller]")]
    public class GroupsController(ChapiService ChapiService, RuntimeInfo RuntimeInfo) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? organization = null, [FromQuery] string? name = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(organization) && !string.IsNullOrEmpty(name))
                {
                    return Ok(await ChapiService.GetGroup(new GroupWithId(organization, name), cancellationToken));
                }
                if (!string.IsNullOrEmpty(organization))
                {
                    return Ok(await ChapiService.GetGroupsByOrganization(organization, cancellationToken));
                }

                return Ok(await ChapiService.GetAllGroups());
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception)
            {
                if (RuntimeInfo.IsDevelopment)
                {
                    throw;
                }

                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Group group, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await ChapiService.CreateGroup(new GroupWithId(group), cancellationToken));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception)
            {
                if (RuntimeInfo.IsDevelopment)
                {
                    throw;
                }

                return StatusCode(500);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Group group, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await ChapiService.PutGroup(new GroupWithId(group), cancellationToken));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception)
            {
                if (RuntimeInfo.IsDevelopment)
                {
                    throw;
                }

                return StatusCode(500);
            }
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody] Group group, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await ChapiService.PatchGroup(new GroupWithId(group), cancellationToken));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception)
            {
                if (RuntimeInfo.IsDevelopment)
                {
                    throw;
                }

                return StatusCode(500);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] GroupMinimalDto group, CancellationToken cancellationToken)
        {
            try
            {
                await ChapiService.DeleteGroup(new GroupWithId(group.ToGroup()), cancellationToken);
            return Ok();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception)
            {
                if (RuntimeInfo.IsDevelopment)
                {
                    throw;
                }

                return StatusCode(500);
            }
        }
    }
}
