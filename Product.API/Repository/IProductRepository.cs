using Microsoft.EntityFrameworkCore;
using Product.API.Data;
using SharedLibrary.Model;

namespace Product.API.Repository
{
    public interface IProductRepository : IRepository<Products>
    {
        // implement the specific method product repository
        Task<IQueryable<Products>> GetAllProducts();
        Task<Products?> GetProductBySku(string sku);
        IQueryable<Products> GetProducts();
    }


    public class ProductRepository : Repository<Products>, IProductRepository
    {
        private readonly ProductDbContext _context;
        public ProductRepository(ProductDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Products?> GetProductBySku(string squ)
        {
           return await _dbSet.Where(Q => Q.SKU == squ).FirstOrDefaultAsync();       
        }

        public Task<IQueryable<Products>> GetAllProducts()
        {
            return Task.FromResult(_dbSet.OrderByDescending(Q => Q.Id).AsQueryable());
        }

        public IQueryable<Products> GetProducts()
        {
            return _dbSet.AsNoTracking().OrderByDescending(Q => Q.Id);
        }
    }
}
