using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aspnetcore.Camps.Model.Entities
{
    public class Camp
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Moniker cannot be empty")]
        public string Moniker { get; set; }

        public string Name { get; set; }
        public DateTime EventDate { get; set; } = DateTime.MinValue;  // start date
        public int Length { get; set; }   // how many days the event last
        public string Description { get; set; }
        public virtual Location Location { get; set; }

        public ICollection<Speaker> Speakers { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; }         // MSSql


        /*ConcurrencyCheck: we may want to configure LastName on Person to be a concurrency token. 
        This means that if one user tries to save some changes to a Person, 
        but another user has changed the LastName then an exception will be thrown. */
    }
}