namespace Chapi.Api.Models.Exceptions.Common
{
    public class BadRequestException: Exception
    {
        private static readonly string DefaultMessage = "Request was not accepted";
        
        public BadRequestException(string message)
            : base($"{DefaultMessage}: {message}")
        { }

        public BadRequestException(Exception inner)
            : base(DefaultMessage, inner)
        { }

        public BadRequestException(DatabaseItem<DatabaseItemWithId> item)
            : base($"{DefaultMessage}: {item.GetId()}")
        { }

        public BadRequestException(DatabaseItem<DatabaseItemWithId> item, Exception inner)
            : base($"{DefaultMessage}: {item.GetId()}", inner)
        { }
    }
}
