namespace Chapi.Api.Models.Configuration
{
    public interface IConfigData<T>
    {

        T ToValidated();
    }
}
