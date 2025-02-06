using Chapi.Api.Models.Users;

namespace Chapi.Api.Models.Groups
{
    public class Group : DatabaseCompliantObject<GroupDto>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Parent { get; set; }

        public Group() : base() { }

        public Group(GroupDto groupDto): base(groupDto)
        {
            if (groupDto.Name == null) throw new ArgumentNullException(nameof(groupDto.Name));

            Id = groupDto.Name;
            Name = groupDto.Name;
            Description = groupDto.Description;
            ProfilePicture = groupDto.ProfilePicture;
            Parent = groupDto.Parent;
        }

        public override GroupDto ToDatabaseDto()
        {
            return new GroupDto
            {
                Name = Name,
                Description = Description,
                ProfilePicture = ProfilePicture,
                Parent = Parent
            };
        }
    }
}
