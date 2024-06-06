using System;
using System.IO;
using CH.CleanArchitecture.Common.Enumerations;

namespace CH.CleanArchitecture.Core.Application.DTOs
{
    public class ResourceDTO
    {
        /// <summary>
        /// The resource data
        /// </summary>
        public BinaryData Data { get; set; }
        public string Name { get; set; }
        public string ContainerName { get; set; }
        public ResourceType Type { get; set; }
        public string URI { get; set; }
        public string Meta { get; set; }
        public string Extension { get; set; }
    }
}
