using CH.CleanArchitecture.Common.Enumerations;

namespace CH.CleanArchitecture.Core.Application.ReadModels
{
    public class ResourceReadModel : IReadModel
    {
        public string Name { get; set; }
        public string ContainerName { get; set; }
        public ResourceType Type { get; set; }
        public string URI { get; set; }
        public string Meta { get; set; }
        public string Extension { get; set; }
        public string Domain { get; set; }
    }
}
