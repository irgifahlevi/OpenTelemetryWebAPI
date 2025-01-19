using Microsoft.EntityFrameworkCore;
using Order.API.Extension;
using Order.API.Model;
using Order.API.Repository;

namespace Order.API.Services
{
    public class OrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IEnumerable<OrderItem>, int)> GetAllOrderItems(int page, int pageSize)
        {
            var query = await _unitOfWork.OrderRepository.GetOrderItems();
            var totalOrder = await query.CountAsync();
            var orders = await query.Skip((page - 1) * pageSize).
                Take(pageSize).ToListAsync();

            return (orders, totalOrder);
        }

        public async Task<OrderItem?> GetOrderItem(int productId)
        {
            return await _unitOfWork.OrderRepository.GetOrderByProductId(productId);
        }

        public async Task AddOrder(OrderItem order)
        {
            try
            {
                var existsOrder = await _unitOfWork.OrderRepository.GetOrderByProductId(order.ProductId);
                if (existsOrder != null)
                {
                    existsOrder.Quantity += order.Quantity;
                    order.OrderDate = DateTime.UtcNow;
                    _unitOfWork.OrderRepository.Update(existsOrder);
                }
                else
                {
                    order.OrderDate = DateTime.UtcNow;
                    await _unitOfWork.OrderRepository.AddAsync(order);
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message, 500);
            }
        }


        public async Task UpdateOrder(OrderItem order)
        {
            try
            {
                var existsOrder = await _unitOfWork.OrderRepository.GetOrderByProductId(order.ProductId);
                if (existsOrder != null)
                {
                    if (existsOrder.Quantity < order.Quantity) 
                    {
                        existsOrder.Quantity = order.Quantity;
                    }
                    else
                    {

                    }
                    
                    order.OrderDate = DateTime.UtcNow;
                    _unitOfWork.OrderRepository.Update(existsOrder);
                    await _unitOfWork.CommitAsync(); 
                }       
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message, 500);
            }
        }

        public async Task DeleteOrder(int id)
        {
            var order = await _unitOfWork.OrderRepository.GetIdAsync(id);
            if (order != null)
            {
                _unitOfWork.OrderRepository.Delete(order);
                await _unitOfWork.CommitAsync();
            }
        }
    }
}
