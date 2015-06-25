namespace Repository.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Product")]
    public partial class Product
    {
        public Product()
        {
            OrderLines = new HashSet<OrderLine>();
        }

        public int ProductId { get; set; }

        [StringLength(80)]
        public string ProductName { get; set; }

        [Column(TypeName = "smallmoney")]
        public decimal? Price { get; set; }

        public bool? Active { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Stock { get; set; }

        public virtual ICollection<OrderLine> OrderLines { get; set; }
    }
}
