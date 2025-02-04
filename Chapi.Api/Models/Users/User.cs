using Newtonsoft.Json;

namespace Chapi.Api.Models.Users
{
    public class UserCreateDto
    {
        public string? Organization { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? ProfilePicture { get; set; }
        public UserAccess[]? Access { get; set; }
    }

    public class User : CosmosDtoBase
    {
        public User() {} // For Serialization

        public User(UserCreateDto userDto)
        {
            Organization = userDto.Organization;
            Email = userDto.Email;
            Name = userDto.Name;
            ProfilePicture = userDto.ProfilePicture;
            Access = userDto.Access;
        }

        [JsonProperty(PropertyName = "organization")]
        public string? Organization { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? ProfilePicture { get; set; }
        public UserAccess[]? Access { get; set; }
    }

    public class UserAccess
    {
        public string? Application { get; set; }
        public string[]? Roles { get; set; }
    }
}
