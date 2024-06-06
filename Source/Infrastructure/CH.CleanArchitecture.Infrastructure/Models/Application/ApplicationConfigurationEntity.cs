﻿using CH.Data.Abstractions;

namespace CH.CleanArchitecture.Infrastructure.Models
{
    /// <summary>
    /// Data entity for application-specific configurations
    /// </summary>
    public class ApplicationConfigurationEntity : DataEntityBase<string>
    {
        /// <summary>
        /// Description of the configuration variable.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Value of the configuration variable.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="Value"/> field contains an encrypted text.
        /// </summary>
        public bool IsEncrypted { get; set; }
    }
}