using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Utilities.Extensions;

namespace Chapi.Api.Models
{
    public class GroupMinimalDto
    {
        public string? Organization { get; set; }
        public string? Name { get; set; }

        public Group ToGroup()
        {
            return new Group()
            {
                Name = Name,
                Organization = Organization
            };
        }
    }
    public class Group : DatabaseItem
    {
        //PartitionKey
        public static bool IdIsOrganization(string? id) => string.IsNullOrEmpty(id) ? false : id.Split(':') is [var left, var right] && left == right;
        public static string IdAsOrganization(string? id) => $"{id}:{id}";

        public bool IsOrganization() => Name == Organization;
        public string? Organization { get; set; }
        public override string GetPartitionKeyFieldName() => nameof(Organization);
        public override string? GetPartitionKey() => Organization;

        //Id
        public string? Name { get; set; }
        public override string GetIdFieldName() => nameof(Name);
        protected override string? MapToId() => $"{Organization}:{Name}";

        public string? Parent { get; set; }
        public string? Description { get; set; }
        public string? ProfilePicture { get; set; }
        public List<string> Users { get; set; } = [];
        public List<ApplicationAccess> Applications { get; set; } = [];
        public List<string> Children { get; set; } = [];


        public void SoftOverwriteWith(Group overwriter)
        {
            Parent = overwriter.Parent ?? Parent;
            Description = overwriter.Description ?? Description;
            ProfilePicture = overwriter.ProfilePicture ?? ProfilePicture;

            foreach (var member in overwriter.Users)
            {
                Users.AddIfNotExists(member);
            }

            foreach (var application in overwriter.Applications)
            {
                var currentApplication = Applications.Find(x => x.Name == application.Name);
                if (currentApplication != null)
                {
                    foreach(var role in currentApplication.Roles)
                    {
                        if (!currentApplication.Roles.Contains(role))
                        {
                            currentApplication.Roles.Add(role);
                        }
                    }
                }
                else
                {
                    Applications.Add(application);
                }
            }

            foreach (var overwriterChild in overwriter.Children)
            {
                Children.AddIfNotExists(overwriterChild);
            }
        }
    }

    public sealed class GroupWithId : Group, IDatabaseItemWithId
    {
        public string? Id { get; set; }

        public GroupWithId(){}

        public GroupWithId(string? id) 
        {
            var idParts = id?.Split(":");
            if(idParts?.Length != 2)
            {
                throw new BadRequestException(this, $"Group initialized with an id that could not be parsed into \"Organization:Name\" \"{id}\"");
            }

            Organization = idParts[0];
            Name = idParts[1];
            Id = GetId();
        }

        public GroupWithId(string? organization, string? name)
        {
            Organization = organization;
            Name = name;
            Id = GetId();
        }

        public GroupWithId(Group? group)
        {
            if (group == null) return;

            Name = group.Name;
            Parent = group.Parent;
            Description = group.Description;
            ProfilePicture = group.ProfilePicture;
            Organization = group.Organization;
            Users = group.Users;
            Applications = group.Applications;
            Children = group.Children;

            Id = group.GetId();
        }
    }
}
