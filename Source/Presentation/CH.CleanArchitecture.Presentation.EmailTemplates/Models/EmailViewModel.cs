using System;
using System.Collections.Generic;
using System.Text;

namespace CH.CleanArchitecture.Presentation.EmailTemplates.Models
{
    /// <summary>
    /// Generic model for the email. Contains any payload needed for the email
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EmailViewModel<T>
    {
        public T Payload { get; set; }
        public CTAViewModel CallToAction { get; set; }
    }
}
