using Chapi.Api.Utilities.Extensions;
using static System.Net.Mime.MediaTypeNames;

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
        //PartitionKey
        public string? Platform { get; set; }
        public override string GetPartitionKeyFieldName() => nameof(Platform);
        public override string? GetPartitionKey() => Platform;

        //Id
        public string? Name { get; set; }
        public override string GetIdFieldName() => nameof(Name);
        protected override string? MapToId() => Name;


        public string? Description { get; set; }
        public Dictionary<string, bool> Users { get; set; } = [];
        public Dictionary<string, bool> Groups { get; set; } = [];
        public List<string> Permissions { get; set; } = [];
        public List<Role> Roles { get; set; } = [];
        public object? Data { get; set; }


        public class Role
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public List<string> Permissions { get; set; } = [];
        }

        public void SoftOverwrite(Application overwriter)
        {
            Description = overwriter.Description;

            if (overwriter.Data != null)
            {
                throw new NotImplementedException("Currently cannot soft overwrite data");
            }

            
            foreach (var overwriterGroup in overwriter.Groups)
            {
                Groups[overwriterGroup.Key] = overwriterGroup.Value;
            }

            foreach (var overwriterPermission in overwriter.Permissions)
            {
                Permissions.AddIfNotExists(overwriterPermission);
            }

            foreach(var overwriterRole in overwriter.Roles)
            {
                var currentRole = Roles.Find(x => x.Name == overwriterRole.Name);
                if (currentRole != null)
                {
                    currentRole.Description = overwriterRole.Description ?? currentRole.Description;

                    foreach (var overwriterPermission in overwriterRole.Permissions)
                    {
                        if (!currentRole.Permissions.Contains(overwriterPermission))
                        {
                            currentRole.Permissions.Add(overwriterPermission);
                        }
                    }
                }
                else
                {
                    Roles.Add(overwriterRole);
                }
            }
        }
    }

    public class ApplicationAccess
    {
        public string? Name { get; set; }
        public List<string> Roles { get; set; } = [];
    }

    public sealed class ApplicationWithId : Application, IDatabaseItemWithId
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
