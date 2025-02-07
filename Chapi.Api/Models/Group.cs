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
    public class Group : DatabaseItem<GroupWithId>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Parent { get; set; }

        internal override string? GetPartitionKeyString() => Parent;
        public override string GetId() => Name ?? throw new ArgumentNullException(nameof(Name));

        internal override GroupWithId CreateInstance()
        {
            return new GroupWithId();
        }
    }

    public class GroupWithId : Group, DatabaseItemWithId
    {
        public string? Id { get; set; }
    }
}
