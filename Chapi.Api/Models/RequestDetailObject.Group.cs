namespace Chapi.Api.Models
{
    public static partial class RequestDetails
    {
        public static RequestDetailObject GroupNotFound(Group group)
        {
            return new RequestDetailObject(group)
            {
                Status = RequestStatus.NotFound,
                Message = $"Group not found"
            };
        }
    }
}
