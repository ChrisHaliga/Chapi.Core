namespace Chapi.Api.Models
{
    public static partial class RequestDetails
    {
        public static RequestDetailObject UserNotFound(User user)
        {
            return new RequestDetailObject(user)
            {
                Status = RequestStatus.NotFound,
                Message = $"User not found"
            };
        }
    }
}
