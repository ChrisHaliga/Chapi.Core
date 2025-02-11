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
    public class UsersController(ChapiService ChapiService, RuntimeInfo RuntimeInfo) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? email = null, [FromQuery] string? organization = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    return Ok(await UserService.GetItemById(email, cancellationToken));
                }
                if (!string.IsNullOrEmpty(organization))
                {
                    return Ok(await UserService.GetItemsByPartitionKey(organization, cancellationToken));
                }
                return Ok(await UserService.GetAllItems());
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
        public async Task<IActionResult> Post([FromBody] User user, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await ChapiService.CreateUser(new UserWithId(user), cancellationToken));
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
        public async Task<IActionResult> Put([FromBody] User user, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await UserService.UpdateItem(new UserWithId(user), true, cancellationToken));
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
        public async Task<IActionResult> Patch([FromBody] User user, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await UserService.UpdateItem(new UserWithId(user), false, cancellationToken));
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
        public async Task<IActionResult> Delete([FromBody] UserMinimalDto userMinimal, CancellationToken cancellationToken)
        {
            try
            {
                await ChapiService.DeleteUser(new UserWithId(userMinimal.ToUser()), cancellationToken);
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
