using CH.CleanArchitecture.Common.Enumerations;

namespace CH.CleanArchitecture.Presentation.WebApp.Models
{
    public class ResourceFormModel
    {
        public string Name { get; set; }
        public string FolderName { get; set; }
        public ResourceType Type { get; set; }
        public string Domain { get; set; }
        public bool IsPublic { get; set; }
    }
}
