namespace Chapi.Api.Models.Users
{
    public class UserDto : DatabaseDto
    {
        public string? Organization { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? ProfilePicture { get; set; }
        public UserAccess[]? Access { get; set; }

        public override string Id()
        {
            if (Email == null) throw new ArgumentNullException(nameof(Email));
            return Email;
        }

        public override string? PartionKey()
        {
            return Organization;
        }
    }
}
