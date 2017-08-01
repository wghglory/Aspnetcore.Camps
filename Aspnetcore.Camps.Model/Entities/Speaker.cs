using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aspnetcore.Camps.Model.Entities
{
    public class Speaker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string PhoneNumber { get; set; }
        public string WebsiteUrl { get; set; }
        public string TwitterName { get; set; }
        public string GitHubName { get; set; }
        public string Bio { get; set; }
        public string HeadShotUrl { get; set; }
        public virtual CampUser User { get; set; }

        public ICollection<Talk> Talks { get; set; }
        public virtual Camp Camp { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; }         // MSSql


        /*ConcurrencyCheck: we may want to configure LastName on Person to be a concurrency token. 
        This means that if one user tries to save some changes to a Person, 
        but another user has changed the LastName then an exception will be thrown. */
    }
}