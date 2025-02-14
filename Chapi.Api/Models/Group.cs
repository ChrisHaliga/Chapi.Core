using Chapi.Api.Utilities.Extensions;
using Microsoft.Azure.Cosmos;

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
        public static readonly string RootOrganization = "organizations";

        //PartitionKey
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
        public Dictionary<string, bool> Members { get; set; } = [];
        public List<ApplicationAccess> Applications { get; set; } = [];
        public List<string> Children { get; set; } = [];


        public void SoftOverwrite(Group overwriter)
        {
            Parent = overwriter.Parent ?? Parent;
            Description = overwriter.Description ?? Description;
            ProfilePicture = overwriter.ProfilePicture ?? ProfilePicture;

            foreach (var member in overwriter.Members)
            {
                Members[member.Key] = member.Value;
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

        public GroupWithId() { }

        public GroupWithId(string? id)
        {
            Id = id;
        }

        public GroupWithId(Group? group)
        {
            if (group == null) return;

            Name = group.Name;
            Parent = group.Parent;
            Description = group.Description;
            ProfilePicture = group.ProfilePicture;
            Organization = group.Organization;
            Members = group.Members;
            Applications = group.Applications;
            Children = group.Children;

            Id = group.GetId();
        }
    }
}
