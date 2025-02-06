namespace Chapi.Api.Models.Users
{
    public class User : DatabaseCompliantObject<UserDto>
    {
        public string? Organization { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? ProfilePicture { get; set; }
        public UserAccess[]? Access { get; set; }

        public User(UserDto userDto): base(userDto)
        {
            Organization = userDto.Organization;
            Email = userDto.Email;
            Name = userDto.Name;
            ProfilePicture = userDto.ProfilePicture;
            Access = userDto.Access;
        }

        public User(): base(){}

        public override DatabaseDto ToDatabaseDto()
        {
            return new UserDto
            {
                Organization = Organization,
                Email = Email,
                Name = Name,
                ProfilePicture = ProfilePicture,
                Access = Access
            };
        }
    }

    public class UserAccess
    {
        public string? Application { get; set; }
        public string[]? Roles { get; set; }
    }
}
