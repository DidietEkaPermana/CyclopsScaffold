namespace Repository.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;

    public partial class Fabrics : DbContext
    {
        public Fabrics()
            : base("name=Fabrics")
        {
        }

        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Occupation> Occupations { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderLine> OrderLines { get; set; }
        public virtual DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>()
                .Property(e => e.FirstName)
                .IsUnicode(false);

            modelBuilder.Entity<Client>()
                .Property(e => e.MiddleName)
                .IsUnicode(false);

            modelBuilder.Entity<Client>()
                .Property(e => e.LastName)
                .IsUnicode(false);

            modelBuilder.Entity<Client>()
                .Property(e => e.Gender)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Client>()
                .Property(e => e.XCode)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Client>()
                .Property(e => e.TelephoneNumber)
                .IsUnicode(false);

            modelBuilder.Entity<Client>()
                .Property(e => e.Street1)
                .IsUnicode(false);

            modelBuilder.Entity<Client>()
                .Property(e => e.Street2)
                .IsUnicode(false);

            modelBuilder.Entity<Client>()
                .Property(e => e.City)
                .IsUnicode(false);

            modelBuilder.Entity<Client>()
                .Property(e => e.ZipCode)
                .IsUnicode(false);

            modelBuilder.Entity<Client>()
                .Property(e => e.Notes)
                .IsUnicode(false);

            modelBuilder.Entity<Occupation>()
                .Property(e => e.OccupationName)
                .IsUnicode(false);

            modelBuilder.Entity<Order>()
                .Property(e => e.OrderStatus)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Order>()
                .HasMany(e => e.OrderLines)
                .WithRequired(e => e.Order)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderLine>()
                .Property(e => e.Qty)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Product>()
                .Property(e => e.ProductName)
                .IsUnicode(false);

            modelBuilder.Entity<Product>()
                .Property(e => e.Price)
                .HasPrecision(10, 4);

            modelBuilder.Entity<Product>()
                .Property(e => e.Stock)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Product>()
                .HasMany(e => e.OrderLines)
                .WithRequired(e => e.Product)
                .WillCascadeOnDelete(false);
        }
    }

    [Table("Client")]
    public partial class Client
    {
        public Client()
        {
            Orders = new HashSet<Order>();
        }

        public int ClientId { get; set; }

        [StringLength(40)]
        public string FirstName { get; set; }

        [StringLength(40)]
        public string MiddleName { get; set; }

        [StringLength(40)]
        public string LastName { get; set; }

        [StringLength(1)]
        public string Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public double? CreditRating { get; set; }

        [StringLength(7)]
        public string XCode { get; set; }

        public int? OccupationId { get; set; }

        [StringLength(20)]
        public string TelephoneNumber { get; set; }

        [StringLength(100)]
        public string Street1 { get; set; }

        [StringLength(100)]
        public string Street2 { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(15)]
        public string ZipCode { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        public string Notes { get; set; }

        public virtual Occupation Occupation { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }

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

    [Table("OrderLine")]
    public partial class OrderLine
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OrderId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LineNumber { get; set; }

        public int ProductId { get; set; }

        [Column(TypeName = "numeric")]
        public decimal Qty { get; set; }

        [Column(TypeName = "numeric")]
        public decimal LineTotal { get; set; }

        public virtual Order Order { get; set; }

        public virtual Product Product { get; set; }
    }

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
