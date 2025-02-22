using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Order.API.DTO;
using Order.API.Extension;
using Order.API.Model;
using Order.API.Repository;
using Order.API.Services;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly ProductService _productService;

        public OrdersController(OrderService orderService, ProductService productService)
        {
            _orderService = orderService;
            _productService = productService;
        }

        // GET: api/Orders/Summary
        [HttpGet("Summary")]
        [EndpointSummary("Get all data order summary")]
        public async Task<ActionResult> GetOrdersSummary([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var stopwatch = Stopwatch.StartNew();

            var (order, totalOrder) = await _orderService.GetAllOrderItems(page, pageSize);

            var pagination = new Pagination
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalOrder / (double)pageSize),
                PageSize = pageSize,
                TotalCount = totalOrder
            };

            var orderSummary = new List<OrderSummary>();

            foreach (var orderItem in order) 
            {
                var product = await _productService.GetSyncProduct(orderItem.ProductId);
                if (product != null)
                    orderSummary.Add(new OrderSummary()
                    { 
                        OrderId =  orderItem.Id,
                        ProductId = product?.Id ?? 0,
                        ProductPrice = product?.Price ?? 0,
                        OrderedQuantity = orderItem.Quantity,
                        ProductName = product?.Name ?? "",
                    });

            }


            stopwatch.Stop();

            var response = ApiResponseHelper.Success(orderSummary, "Order retrieved successfully", stopwatch.Elapsed.TotalMilliseconds, pagination);
            return Ok(response);
        }


        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var stopwatch = Stopwatch.StartNew();

            var (order, totalOrder) = await _orderService.GetAllOrderItems(page, pageSize);

            var pagination = new Pagination
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalOrder / (double)pageSize),
                PageSize = pageSize,
                TotalCount = totalOrder
            };

            stopwatch.Stop();

            var response = ApiResponseHelper.Success(order, "Order retrieved successfully", stopwatch.Elapsed.TotalMilliseconds, pagination);
            return Ok(response);
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItem>> GetOrder(int id)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var order = await _orderService.GetOrderItem(id);

            if (order == null)
            {
                throw new AppException($"Order with ID {id} not found", 404);
            }
            stopwatch.Stop();

            var response = ApiResponseHelper.Success(order, "Order retrieved successfully", stopwatch.Elapsed.TotalMilliseconds);
            return Ok(response);
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrders(int id, OrderItem orders)
        {
            // to do implement code
            return Ok();
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<OrderItemViewModel>> PostOrders(OrderRequest orders)
        {
            var executeTime = Stopwatch.StartNew();
            await _orderService.AddOrder(orders);
            executeTime.Stop();
            return CreatedAtAction(
                "GetOrder",           // Name of the action to get the created resource (e.g., GetOrder)
                new { id = orders.Id },  // Route parameters for the GetOrder action (in this case, the product Id)
                ApiResponseHelper.Success(orders, "Order created successfully", executeTime.Elapsed.TotalMilliseconds)  // Return custom response
            );
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrders(int id)
        {
            // to do implement code
            return Ok();
        }
    }
}
