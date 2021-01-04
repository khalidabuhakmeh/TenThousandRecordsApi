using System.Collections.Generic;
using System.Linq;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace TenThousandRecordsApi.Models
{
    public class Database : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public Database(DbContextOptions<Database> options)
            :base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 10,000
            var productIds = 1;
            var products = new Faker<Product>()
                .RuleFor(m => m.Id, () => productIds++)
                .RuleFor(m => m.Name, f => f.Commerce.ProductName())
                .RuleFor(m => m.Department, f => f.Commerce.Department())
                .RuleFor(m => m.Category, f => f.Commerce.Categories(1).First())
                .RuleFor(m => m.Color, f => f.Commerce.Color())
                .RuleFor(m => m.Price, f => f.Finance.Amount(1))
                .RuleFor(m => m.Description, f => f.Commerce.ProductDescription())
                .Generate(10_000);
            
            // Seed the database
            modelBuilder.Entity<Product>()
                .HasData(products);
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string Category { get; set; }
        public string Color { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }

    public class InMemory
    {
        public InMemory(List<Product> products)
        {
            Products = products.AsReadOnly();
        }

        public IReadOnlyList<Product> Products { get; }
    }
}