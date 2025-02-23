using Microsoft.EntityFrameworkCore;
using SharedLibrary.Model;

namespace Product.API.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext>options) : base(options)
        { }

        DbSet<Products> Products { get; set; }
    }
}
