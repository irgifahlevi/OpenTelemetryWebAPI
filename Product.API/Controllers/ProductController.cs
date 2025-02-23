using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Product.API.Extension;
using Product.API.Services;
using SharedLibrary.Model;

namespace Product.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductController(ProductService service)
        {
            _service = service;
        }


        // GET: api/Products
        [HttpGet]
        [EndpointSummary("Get all data products")]
        public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {

            var (products, totalCount) = await _service.GetProductsWithPaginationAsync(page, pageSize);

            var pagination = new Pagination
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                PageSize = pageSize,
                TotalCount = totalCount
            };

            var response = ApiResponseHelper.Success(products, "Products retrieved successfully", pagination);
            return Ok(response);
        }

        // GET: api/Product/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Products>> GetProductId(int id)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var product = await _service.GetProductIdAsync(id);

            if (product == null)
            {
                throw new AppException($"Product with ID {id} not found", 404);
            }
            stopwatch.Stop();

            var response = ApiResponseHelper.Success(product, "Products retrieved successfully");
            return Ok(response);
        }

        // PUT: api/Product/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, [FromBody] Products product)
        {
            if (id != product.Id)
            {
                throw new AppException($"Request invalid", 400);
            }

            var productData = await _service.GetProductIdAsync(id);

            if (productData == null)
            {
                throw new AppException($"Product with ID {id} not found", 404);
            }

            await _service.UpdateProductAsync(product);
            return NoContent();
        }

        // POST: api/Product
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Products>> PostProduct(Products product)
        {
            await _service.AddProductAsync(product);

            // Return CreatedAtAction response
            return CreatedAtAction(
                "GetProductId",           // Name of the action to get the created resource (e.g., GetProductId)
                new { id = product.Id },  // Route parameters for the GetProductId action (in this case, the product Id)
                ApiResponseHelper.Success(product, "Product created successfully")  // Return custom response
            );
        }

        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _service.GetProductIdAsync(id);

            if (product == null)
            {
                throw new AppException($"Product with ID {id} not found", 404);
            }

            await _service.DeleteProductAsync(id);
            return NoContent();
        }
    }
}
