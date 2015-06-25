namespace Repository.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

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
}
