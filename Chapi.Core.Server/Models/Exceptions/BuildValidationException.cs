namespace Chapi.Api.Models.Exceptions
{
    public class BuildValidationException : AggregateException
    {
        private static readonly string DefaultMessage = "Build Validation Error";

        public BuildValidationException(string message) : base($"{DefaultMessage}: {message}") { }
        public BuildValidationException(string message, Exception inner) : base($"{DefaultMessage}: {message}", inner) { }
    }
}
