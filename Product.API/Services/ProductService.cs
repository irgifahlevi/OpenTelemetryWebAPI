using Microsoft.EntityFrameworkCore;
using Product.API.Extension;
using Product.API.Model;
using Product.API.Repository;

namespace Product.API.Services
{
    public class ProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Products>> GetProductsAsync()
        {
            return await _unitOfWork.ProductRepository.GetAllAsync();   
        }

        public async Task<(IEnumerable<Products>, int)> GetProductsWithPaginationAsync(int page, int pageSize)
        {
            var query = await _unitOfWork.ProductRepository.GetAllProducts();
            var totalCount = await query.CountAsync();

            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (products, totalCount);
        }

        public async Task<Products> GetProductIdAsync(int id)
        {
            return await _unitOfWork.ProductRepository.GetIdAsync(id);        
        }

        public async Task AddProductAsync(Products product)
        {
            try
            {
                var checkData = await _unitOfWork.ProductRepository.GetProductBySku(product.SKU);
                if (checkData != null) 
                {
                    checkData.Name = product.Name;
                    checkData.Description = product.Description;
                    checkData.Price = product.Price;

                    _unitOfWork.ProductRepository.Update(checkData);
                }
                else
                {
                    await _unitOfWork.ProductRepository.AddAsync(product);
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
