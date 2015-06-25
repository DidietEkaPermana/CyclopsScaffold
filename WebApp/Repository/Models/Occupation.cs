namespace Repository.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Occupation")]
    public partial class Occupation
    {
        public Occupation()
        {
            Clients = new HashSet<Client>();
        }

        public int OccupationId { get; set; }

        [StringLength(60)]
        public string OccupationName { get; set; }

        public virtual ICollection<Client> Clients { get; set; }
    }
}
