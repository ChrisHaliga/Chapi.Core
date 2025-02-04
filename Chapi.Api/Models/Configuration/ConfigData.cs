namespace Chapi.Api.Models.Configuration
{
    internal abstract class ConfigData
    {
        private bool _isValid = false;

        public bool IsValid => _isValid || Validation();

        private bool Validation()
        {
            _isValid = Validate();
            return _isValid;
        }
        protected abstract bool Validate();
    }

    internal interface IConfigData<T>
    {

        T ToValidated();
    }
}
