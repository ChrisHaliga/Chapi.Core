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
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Parent { get; set; }

        public override string? GetPartitionKey() => Parent;
        protected override string? MapToId() => Name;
        public GroupWithId ToGroupWithId()
        {
            return new GroupWithId(this);
        }
    }

    public class GroupWithId : Group, IDatabaseItemWithId
    {
        public string? Id { get; set; }

        public GroupWithId() { }

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
