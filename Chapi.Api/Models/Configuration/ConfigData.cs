namespace Chapi.Api.Models.Configuration
{
    internal interface IConfigData<T>
    {

        T ToValidated();
    }
}
