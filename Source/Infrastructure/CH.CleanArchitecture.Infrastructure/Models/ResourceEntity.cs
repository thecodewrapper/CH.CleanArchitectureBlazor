using CH.Data.Abstractions;
using CH.CleanArchitecture.Common.Enumerations;

namespace CH.CleanArchitecture.Infrastructure.Models
{
    internal class ResourceEntity : DataEntityBase<string>
    {
        public string Name { get; set; }
        public string ContainerName { get; set; }
        public ResourceType Type { get; set; }
        public string URI { get; set; }
        public string Meta { get; set; }
        public string Extension { get; set; }
    }
}