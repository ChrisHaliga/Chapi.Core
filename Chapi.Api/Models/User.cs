namespace Chapi.Api.Models
{
    public class UserMinimalDto
    {
        public string? Organization { get; set; } //Organizations are Groups with parent = "organizations"
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
        public List<ApplicationAccess> Applications { get; set; } = [];
        public List<string> Groups { get; set; } = [];

        public override string? GetPartitionKey() => Organization;
        protected override string? MapToId() => Email;
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
            Applications = user.Applications;
            Id = user.GetId();
        }
    }
}
