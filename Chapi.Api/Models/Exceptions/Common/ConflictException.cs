namespace Chapi.Api.Models.Exceptions.Common
{
    public class ConflictException : Exception
    {
        private static readonly string DefaultMessage = "Conflict";

        public ConflictException()
           : base($"{DefaultMessage}")
        { }

        public ConflictException(string message)
            : base($"{DefaultMessage}: {message}")
        { }

        public ConflictException(string message, Exception inner)
            : base($"{DefaultMessage}: {message}", inner)
        { }

        public ConflictException(Exception inner)
            : base(DefaultMessage, inner)
        { }

        public ConflictException(IDatabaseItem? item)
            : base($"{DefaultMessage}: {item?.GetId() ?? "null"}")
        { }

        public ConflictException(IDatabaseItem? item, Exception inner)
            : base($"{DefaultMessage}: {item?.GetId() ?? "null"}", inner)
        { }
    }
}
