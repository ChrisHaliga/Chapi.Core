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

    public class User : DatabaseItem
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

        public override string? GetPartitionKey() => Organization;
        protected override string? MapToId() => Email;

        public UserWithId ToUserWithId()
        {
            return new UserWithId(this);
        }
    }

    public class UserWithId : User, IDatabaseItemWithId
    {
        public string? Id { get; set; }

        public UserWithId() { }

        public UserWithId(User? user)
        {
            if (user == null) return;

            Organization = user.Organization;
            Email = user.Email;
            Name = user.Name;
            ProfilePicture = user.ProfilePicture;
            Access = user.Access;
            Id = user.GetId();
        }
    }
}
