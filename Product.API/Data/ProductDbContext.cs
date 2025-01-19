using Microsoft.EntityFrameworkCore;
using Product.API.Model;

namespace Product.API.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext>options) : base(options)
        { }

        DbSet<Products> Products { get; set; }
    }
}
