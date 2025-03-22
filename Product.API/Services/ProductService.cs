using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Product.API.Extension;
using Product.API.Repository;
using SharedLibrary.Model;
using System.Diagnostics;

namespace Product.API.Services
{
    public class ProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private static ActivitySource ActivitySource;
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        public ProductService(IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint, IConfiguration configuration, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
            _configuration = configuration;
            _cache = cache;
            var serviceName = _configuration.GetValue<string>("AppSettings:ServiceName");
            var serviceVersion = _configuration.GetValue<string>("AppSettings:ServiceVersion");

            ActivitySource = new ActivitySource(serviceName!, serviceVersion!);
        }

        public async Task<IEnumerable<Products>> GetProductsAsync()
        {
            return await _unitOfWork.ProductRepository.GetAllAsync();   
        }

        public async Task<(IEnumerable<Products>, int)> GetProductsWithPaginationAsync(int page, int pageSize)
        {
            string cacheKey = $"products_page_{page}_size_{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out (IEnumerable<Products>, int) cachedData))
            {
                var query = _unitOfWork.ProductRepository.GetProducts();
                var totalCount = await query.CountAsync();

                var products = await query.Skip((page - 1) * pageSize)
                                          .Take(pageSize)
                                          .ToListAsync();

                cachedData = (products, totalCount);

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                };

                _cache.Set(cacheKey, cachedData, cacheOptions);
            }

            return cachedData;
        }

        public async Task<Products> GetProductIdAsync(int id)
        {
            return await _unitOfWork.ProductRepository.GetIdAsync(id);        
        }

        public async Task AddProductAsync(Products product)
        {
            try
            {
                var existProduct = await _unitOfWork.ProductRepository.GetProductBySku(product.SKU);
                if (existProduct != null) 
                {
                    existProduct.Name = product.Name;
                    existProduct.Description = product.Description;
                    existProduct.Price = product.Price;

                    _unitOfWork.ProductRepository.Update(existProduct);
                }
                else
                {
                    await _unitOfWork.ProductRepository.AddAsync(product);
                }

                Products sourceData = existProduct != null ? existProduct : product;

                // publish event to rabitmq
                using var activity = ActivitySource.StartActivity("PublishProductEvent", ActivityKind.Producer);
                if (activity != null)
                {
                    activity.SetTag("messaging.system", "rabbitmq");
                    activity.SetTag("messaging.destination", "product-created");
                    activity.SetTag("product.name", sourceData.Name);

                    var headers = new Dictionary<string, object>();

                    Propagator.Inject(new PropagationContext(activity.Context, Baggage.Current), headers, (headers, key, value) =>
                    {
                        headers[key] = value;
                    });

                    await _publishEndpoint.Publish(sourceData, ctx =>
                    {
                        foreach (var header in headers)
                        {
                            ctx.Headers.Set(header.Key, header.Value);
                        }
                    });
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message, 500);
            }
        }

        public async Task UpdateProductAsync(Products product)
        {
            try
            {
                var productData = await GetProductIdAsync(product.Id);

                productData.Name = product.Name;
                productData.Description = product.Description;
                productData.Price = product.Price;

                _unitOfWork.ProductRepository.Update(productData);
                await _unitOfWork.CommitAsync();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.ProductRepository.GetIdAsync(id);
            if (product != null)
            {
                _unitOfWork.ProductRepository.Delete(product);
                await _unitOfWork.CommitAsync();
            }
        }
    }
}
