namespace Chapi.Api.Utilities
{
    public interface IBuildValidator
    {
       void Validate();
    }

    public static class BuildValidated
    {
        public static void Validate()
        {
            var implementations = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IBuildValidator).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var type in implementations)
            {
                var instance = (IBuildValidator?)Activator.CreateInstance(type);
                instance?.Validate();
            }
        }
    }
}
