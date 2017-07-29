using System;
using System.ComponentModel.DataAnnotations;

namespace Aspnetcore.Camps.Api.ViewModels
{
    public class CampViewModel
    {
        public string Url { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public string Moniker { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        public string Name { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required]
        [MinLength(25)]
        [MaxLength(4096)]
        public string Description { get; set; }

        // below is convention-based flattened location.
        // Camp has navigation property "Location", and if we add `Location` before the properties in Location entity
        // Automapper knows this rule, and it will do the mappings automatically
        public string LocationAddress1 { get; set; }
        public string LocationAddress2 { get; set; }
        public string LocationAddress3 { get; set; }
        public string LocationCityTown { get; set; }
        public string LocationStateProvince { get; set; }
        public string LocationPostalCode { get; set; }
        public string LocationCountry { get; set; }
    }
}