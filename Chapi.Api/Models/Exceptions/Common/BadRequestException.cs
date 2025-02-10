namespace Chapi.Api.Models.Exceptions.Common
{
    public class BadRequestException: Exception
    {
        private static readonly string DefaultMessage = "Request was not accepted";

        public BadRequestException()
            : base($"{DefaultMessage}")
        { }

        public BadRequestException(string message)
            : base($"{DefaultMessage}: {message}")
        { }

        public BadRequestException(string message, Exception inner)
            : base($"{DefaultMessage}: {message}", inner)
        { }

        public BadRequestException(Exception inner)
            : base(DefaultMessage, inner)
        { }

        public BadRequestException(IDatabaseItem? item, string reason)
            : base($"{DefaultMessage}: {item?.GetId() ?? "null"} - Reason: {reason}")
        { }

        public BadRequestException(IDatabaseItem? item, string reason, Exception inner)
            : base($"{DefaultMessage}: {item?.GetId() ?? "null"} - Reason: {reason}", inner)
        { }
    }
}
