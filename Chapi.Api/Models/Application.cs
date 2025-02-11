namespace Chapi.Api.Models
{
    public class ApplicationMinimalDto
    {
        public string? Name { get; set; }
        public string? Platform { get; set; }

        public Application ToApplication()
        {
            return new Application()
            {
                Name = Name,
                Platform = Platform
            };
        }
    }

    public class Application : DatabaseItem
    {
        public string? Name { get; set; }
        public string? Platform { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, bool> Groups { get; set; } = [];
        public List<string> Permissions { get; set; } = [];
        public List<Role> Roles { get; set; } = [];
        public object? Data { get; set; }

        protected override string? MapToId() => Name;
        public override string? GetPartitionKey() => Platform;

        public class Role
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public List<string> Permissions { get; set; } = [];
        }

        public void AddGroup(string id) { Groups[id] = true; }
        public bool RemoveGroup(string id, bool hard = false)
        {
            if (Groups.ContainsKey(id))
            {
                if (hard)
                {
                    Groups.Remove(id);
                }
                else
                {
                    Groups[id] = false;
                }
                return true;
            }
            return false;
        }
    }

    public class ApplicationAccess
    {
        public string? Name { get; set; }
        public string[] Roles { get; set; } = [];
    }

    public class ApplicationWithId : Application, IDatabaseItemWithId
    {
        public string? Id { get; set; }

        public ApplicationWithId() { }

        public ApplicationWithId(string? id)
        {
            Id = id;
        }

        public ApplicationWithId(Application? app)
        {
            if (app == null) return;
            Name = app.Name;
            Platform = app.Platform;
            Description = app.Description;
            Groups = app.Groups;
            Permissions = app.Permissions;
            Roles = app.Roles;
            Data = app.Data;
            
            Id = app.GetId();
        }
    }
}
