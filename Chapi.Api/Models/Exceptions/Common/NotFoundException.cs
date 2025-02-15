using System;

namespace Chapi.Api.Models.Exceptions.Common
{
    public class NotFoundException : Exception
    {
        private static readonly string DefaultMessage = "No items found";

        public NotFoundException()
            : base($"{DefaultMessage}")
        { }

        public NotFoundException(string message)
            : base($"{DefaultMessage}: {message}")
        { }

        public NotFoundException(string message, Exception inner)
            : base($"{DefaultMessage}: {message}", inner)
        { }

        public NotFoundException(Exception? inner = null)
            : base(DefaultMessage, inner)
        { }

        public NotFoundException(IDatabaseItem? item, string reason)
            : base($"{DefaultMessage}: {item?.GetId() ?? "null"} - Reason: {reason}")
        { }

        public NotFoundException(IDatabaseItem? item, string reason, Exception inner)
            : base($"{DefaultMessage}: {item?.GetId() ?? "null"} - Reason: {reason}", inner)
        { }
    }
}
