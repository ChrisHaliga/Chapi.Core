using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Chapi.Api.Models
{
    public class RequestDetailObject
    {
        public RequestDetailObject(object? data)
        {
            if(data != null)
            {
                Data = JsonConvert.SerializeObject(data);
            }
        }

        public RequestDetailObject(){}

        public RequestDetailObject? Parent { get; set; }
        public RequestStatus? Status { get; set; }
        public string? Message { get; set; }
        public string? Data { get; }

        public static IActionResult RequestStatusToActionResult(RequestDetailObject requestDetail)
        {
            return requestDetail.Status switch
            {
                RequestStatus.Success => new OkObjectResult(requestDetail),
                RequestStatus.NotFound => new NotFoundObjectResult(requestDetail),
                RequestStatus.BadRequest => new BadRequestObjectResult(requestDetail),
                RequestStatus.Unauthorized => new UnauthorizedObjectResult(requestDetail),
                _ => new ObjectResult(requestDetail) { StatusCode = 500 }
            };
        }
    }

    public static partial class RequestDetails
    {
        public static RequestDetailObject Success(object? obj = null)
        {
            return new RequestDetailObject(obj)
            {
                Status = RequestStatus.Success,
                Message = "Success"
            };
        }

        public static RequestDetailObject NotFound(object? obj = null)
        {
            return new RequestDetailObject(obj)
            {
                Status = RequestStatus.NotFound,
                Message = $"Not found"
            };
        }
    }

    public enum RequestStatus
    {
        Success,
        NotFound,
        BadRequest,
        Unauthorized,
        Unknown
    }
}
