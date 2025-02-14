using Chapi.Api.Utilities.Extensions;
using static Chapi.Api.Models.Application;

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
        public static readonly string DefaultOrganization = "unassigned";

        //PartitionKey
        public string? Organization { get; set; }
        public override string GetPartitionKeyFieldName() => nameof(Organization);
        public override string? GetPartitionKey() => Organization;


        //Id
        public string? Email { get; set; }
        public override string GetIdFieldName() => nameof(Email);
        protected override string? MapToId() => Email;


        public string? Name { get; set; }
        public string? ProfilePicture { get; set; }
        public List<ApplicationAccess> Applications { get; set; } = [];
        public List<string> Groups { get; set; } = [];



        public void SoftOverwrite(User overwriter)
        {
            Name = overwriter.Name ?? Name;
            ProfilePicture = overwriter.ProfilePicture ?? ProfilePicture;

            foreach (var application in overwriter.Applications)
            {
                var currentApplication = Applications.Find(x => x.Name == application.Name);
                if (currentApplication != null)
                {
                    foreach (var role in currentApplication.Roles)
                    {
                        currentApplication.Roles.AddIfNotExists(role);
                    }
                }
                else
                {
                    Applications.Add(application);
                }
            }

            foreach(var group in overwriter.Groups)
            {
                Groups.AddIfNotExists(group);
            }
        }
    }

    public sealed class UserWithId : User, IDatabaseItemWithId
    {
        public string? Id { get; set; }

        public UserWithId() { }

        public UserWithId(string? id)
        {
            Id = id;
        }

        public UserWithId(User? user)
        {
            if (user == null) return;

            Organization = user.Organization;
            Email = user.Email;
            Name = user.Name;
            ProfilePicture = user.ProfilePicture;
            Applications = user.Applications;
            Groups = user.Groups;

            Id = user.GetId();
        }
    }
}
