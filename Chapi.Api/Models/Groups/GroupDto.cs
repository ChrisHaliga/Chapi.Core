namespace Chapi.Api.Models.Groups
{
    public class GroupDto : DatabaseDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Parent { get; set; }

        public override string Id()
        {
            if (Name == null) throw new ArgumentNullException(nameof(Name));
            return Name;
        }

        public override string? PartionKey()
        {
            if (Name == null) throw new ArgumentNullException(nameof(Name));
            return Name;
        }
    }
}
