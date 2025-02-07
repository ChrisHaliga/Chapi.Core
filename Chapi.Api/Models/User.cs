namespace Chapi.Api.Models
{
    public class UserMinimalDto
    {
        public string? Organization { get; set; }
        public string? Email { get; set; }

        public User ToUser()
        {
            return new User()
            {
                Organization = Organization,
                Email = Email
            };
        }
    }

    public class User : DatabaseItem<UserWithId>
    {
        public string? Organization { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? ProfilePicture { get; set; }
        public UserAccess[]? Access { get; set; }

        public class UserAccess
        {
            public string? Application { get; set; }
            public string[]? Roles { get; set; }
        }
        internal override string? GetPartitionKeyString() => Organization;
        public override string GetId() => Email ?? throw new ArgumentNullException(nameof(Email));

        internal override UserWithId CreateInstance()
        {
            return new UserWithId();
        }
    }

    public class UserWithId : User, DatabaseItemWithId
    {
        public string? Id { get; set; }
    }
}
