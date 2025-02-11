using Microsoft.Azure.Cosmos;

namespace Chapi.Api.Models
{
    public class GroupMinimalDto
    {
        public string? Parent { get; set; }
        public string? Name { get; set; }

        public Group ToGroup()
        {
            return new Group()
            {
                Name = Name,
                Parent = Parent
            };
        }
    }
    public class Group : DatabaseItem
    {
        public static readonly string OrganizationId = "organizations";
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Parent { get; set; }
        public Dictionary<string, bool> Members { get; set; } = [];
        public List<ApplicationAccess> Applications { get; set; } = [];

        public override string? GetPartitionKey() => Parent;
        protected override string? MapToId() => Name;

        public void AddMember(string id) { Members[id] = true; }
        public bool RemoveMember(string id, bool hard = false)
        {
            if (Members.ContainsKey(id))
            {
                if (hard)
                {
                    Members.Remove(id);
                }
                else
                {
                    Members[id] = false;
                }
                return true;
            }
            return false;
        }
    }

    public class GroupWithId : Group, IDatabaseItemWithId
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
            Description = group.Description;
            ProfilePicture = group.ProfilePicture;
            Parent = group.Parent;
            Id = group.GetId();
        }
    }
}
