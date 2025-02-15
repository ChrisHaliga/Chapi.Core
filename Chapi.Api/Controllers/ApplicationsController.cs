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
    public class ApplicationsController(ChapiService ChapiService, RuntimeInfo RuntimeInfo) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? name = null, [FromQuery] string? platform = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    return Ok(await ChapiService.GetApplication(new ApplicationWithId(name), cancellationToken));
                }
                if (!string.IsNullOrEmpty(platform))
                {
                    return Ok(await ChapiService.GetApplicationsByPlatform(platform, cancellationToken));
                }
                return Ok(await ChapiService.GetAllApplications());
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
                return Ok(await ChapiService.CreateApplication(new ApplicationWithId(application), cancellationToken));
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
                return Ok(await ChapiService.PutApplication(new ApplicationWithId(application), cancellationToken));
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
                return Ok(await ChapiService.PatchApplication(new ApplicationWithId(application), cancellationToken));
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
                await ChapiService.DeleteApplication(new ApplicationWithId(applicationMinimal.ToApplication()), cancellationToken);
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
