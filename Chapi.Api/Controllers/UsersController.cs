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
    public class UsersController(UserApiService ApiService, RuntimeInfo RuntimeInfo) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? email = null, [FromQuery] string? organization = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    return Ok(await ApiService.GetItem(new UserWithId(email), cancellationToken));
                }
                if (!string.IsNullOrEmpty(organization))
                {
                    return Ok(await ApiService.GetItemByPartition(organization, cancellationToken));
                }
                return Ok(await ApiService.GetAllItems(cancellationToken));
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
        public async Task<IActionResult> Post([FromBody] User user, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await ApiService.CreateItem(new UserWithId(user), cancellationToken));
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
        public async Task<IActionResult> Put([FromBody] User user, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await ApiService.PutItem(new UserWithId(user), cancellationToken));
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
        public async Task<IActionResult> Patch([FromBody] User user, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await ApiService.PatchItem(new UserWithId(user), cancellationToken));
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
        public async Task<IActionResult> Delete([FromBody] UserMinimalDto userMinimal, CancellationToken cancellationToken)
        {
            try
            {
                await ApiService.DeleteItem(new UserWithId(userMinimal.ToUser()), cancellationToken);
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
