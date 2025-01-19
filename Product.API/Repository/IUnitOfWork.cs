using Product.API.Data;

namespace Product.API.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository ProductRepository { get; }
        Task<int> CommitAsync();
    }


    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProductDbContext _context;
        private IProductRepository _productRepository;

        public UnitOfWork(ProductDbContext context)
        {
            _context = context;
            _productRepository = new ProductRepository(_context);
        }
        public IProductRepository ProductRepository => _productRepository;

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
