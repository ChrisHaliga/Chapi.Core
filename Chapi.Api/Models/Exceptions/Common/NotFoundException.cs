using System;

namespace Chapi.Api.Models.Exceptions.Common
{
    public class NotFoundException : Exception
    {
        private static readonly string DefaultMessage = "No items found";

        public NotFoundException(string message)
            : base($"{DefaultMessage}: {message}")
        { }

        public NotFoundException(Exception? inner = null)
            : base(DefaultMessage, inner)
        { }

        public NotFoundException(DatabaseItem<DatabaseItemWithId> item)
            : base($"{DefaultMessage}: {item.GetId()}")
        { }

        public NotFoundException(DatabaseItem<DatabaseItemWithId> item, Exception inner)
            : base($"{DefaultMessage}: {item.GetId()}", inner)
        { }
    }
}
