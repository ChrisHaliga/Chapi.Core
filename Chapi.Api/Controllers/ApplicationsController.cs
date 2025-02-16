using Chapi.Api.Middleware;
using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.ApiServices;
using Microsoft.AspNetCore.Mvc;

namespace Chapi.Api.Controllers
{
    [ApiKeyAuthorization]
    [ApiController]
    [Route("[controller]")]
    public class ApplicationsController(ApplicationApiService ApiService, RuntimeInfo RuntimeInfo) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? name = null, [FromQuery] string? platform = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    return Ok(await ApiService.GetItem(new ApplicationWithId(name), cancellationToken));
                }
                if (!string.IsNullOrEmpty(platform))
                {
                    return Ok(await ApiService.GetItemByPartition(platform, cancellationToken));
                }
                return Ok(await ApiService.GetAllItems());
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch(BadRequestException e)
            {
                return BadRequest(e.Message);
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
                return Ok(await ApiService.CreateItem(new ApplicationWithId(application), cancellationToken));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BadRequestException e)
            {
                return BadRequest(e.Message);
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
                return Ok(await ApiService.PutItem(new ApplicationWithId(application), cancellationToken));
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
        public async Task<IActionResult> Patch([FromBody] Application application, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await ApiService.PatchItem(new ApplicationWithId(application), cancellationToken));
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
        public async Task<IActionResult> Delete([FromBody] ApplicationMinimalDto applicationMinimal, CancellationToken cancellationToken)
        {
            try
            {
                await ApiService.DeleteItem(new ApplicationWithId(applicationMinimal.ToApplication()), cancellationToken);
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
