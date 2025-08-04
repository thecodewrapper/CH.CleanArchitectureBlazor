using System;
using CH.CleanArchitecture.Common.Enumerations;

namespace CH.CleanArchitecture.Core.Application.ReadModels
{
    public class ResourceReadModel : IReadModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ContainerName { get; set; }
        public ResourceType Type { get; set; }
        public string URI { get; set; }
        public string Meta { get; set; }
        public string Extension { get; set; }
        public string Domain { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}
