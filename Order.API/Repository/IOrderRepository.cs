using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Order.API.Model;

namespace Order.API.Repository
{
    public interface IOrderRepository : IRepository<OrderItem>
    {
        Task<IQueryable<OrderItem>> GetOrderItems();
        Task<OrderItem?> GetOrderByProductId(int id);
    }

    public class OrderRepository : Repository<OrderItem>, IOrderRepository
    {
        public OrderRepository(OrderDbContext context) : base(context)
        {}
        public async Task<OrderItem?> GetOrderByProductId(int id)
        {
           return await _dbSet.Where(Q => Q.ProductId == id).FirstOrDefaultAsync();
        }

        public Task<IQueryable<OrderItem>> GetOrderItems()
        {
            return Task.FromResult(_dbSet.OrderBy(Q => Q.Id).AsQueryable());
        }
    }

}
