namespace Repository.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Order")]
    public partial class Order
    {
        public Order()
        {
            OrderLines = new HashSet<OrderLine>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OrderId { get; set; }

        public int? ClientId { get; set; }

        public DateTime? OrderDate { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? OrderTotal { get; set; }

        [StringLength(1)]
        public string OrderStatus { get; set; }

        public virtual Client Client { get; set; }

        public virtual ICollection<OrderLine> OrderLines { get; set; }
    }
}
