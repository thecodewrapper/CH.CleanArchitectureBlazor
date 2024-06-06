using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using CH.CleanArchitecture.Presentation.WebApp.ModelBinders;

namespace CH.CleanArchitecture.Presentation.WebApp.Models
{
    public class SubscriptionPlanFormModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Subtitle { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(1, 365)]
        public uint DurationInDays { get; set; }

        [Required]
        [Range(0, 10000)]
        [BindProperty(BinderType = typeof(DecimalCommaModelBinder))]
        public decimal Cost { get; set; }

        [Required]
        public string CostSubtitle { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsUpscalingEnabled { get; set; }
        public uint DaysBeforeExpiryNotification { get; set; }

        [Required]
        [Range(0, 365)]
        public uint GracePeriodInDays { get; set; }
    }
}
