using Chapi.Api.Middleware;
using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.CrudServices;
using Microsoft.AspNetCore.Mvc;

namespace Chapi.Api.Controllers
{
    [ApiKeyAuthorization]
    [ApiController]
    [Route("[controller]")]
    public class ApplicationsController(ApplicationService ApplicationService, RuntimeInfo RuntimeInfo) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? name = null, [FromQuery] string? platform = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    return Ok(await ApplicationService.GetItemById(name, cancellationToken));
                }
                if (!string.IsNullOrEmpty(platform))
                {
                    return Ok(await ApplicationService.GetItemsByPartitionKey(platform, cancellationToken));
                }
                return Ok(await ApplicationService.GetAllItems());
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch(BadRequestException)
            {
                return BadRequest();
            }
            catch(Exception)
            {
                if(RuntimeInfo.IsDevelopment)
                {
                    throw;
                }

                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Application application, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await ApplicationService.CreateItem(new ApplicationWithId(application), cancellationToken));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BadRequestException)
            {
                return BadRequest();
            }
            catch (ConflictException)
            {
                return Conflict();
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
        public async Task<IActionResult> Put([FromBody] Application application, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await ApplicationService.PutItem(new ApplicationWithId(application), cancellationToken));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BadRequestException)
            {
                return BadRequest();
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
        public async Task<IActionResult> Patch([FromBody] Application application, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await ApplicationService.UpdateItem(new ApplicationWithId(application), false, cancellationToken));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BadRequestException)
            {
                return BadRequest();
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
        public async Task<IActionResult> Delete([FromBody] ApplicationMinimalDto applicationMinimal, CancellationToken cancellationToken)
        {
            try
            {
                await ApplicationService.DeleteItem(new ApplicationWithId(applicationMinimal.ToApplication()), cancellationToken);
                return Ok();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BadRequestException)
            {
                return BadRequest();
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
