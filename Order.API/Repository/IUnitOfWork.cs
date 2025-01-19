using Order.API.Data;

namespace Order.API.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository OrderRepository { get; }
        Task<int> CommitAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrderDbContext _context;
        private IOrderRepository _orderRepository;

        public UnitOfWork(OrderDbContext context)
        {
            _context = context;
            _orderRepository = new OrderRepository(_context);
        }
        public IOrderRepository OrderRepository => _orderRepository;

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
