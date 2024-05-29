using Fiorello_PB101.Models;

namespace Fiorello_PB101.Services.Interfaces
{
    public interface IProductImageService
    {
        Task<ProductImage> GetByIdAsync(int  id);
        Task<List<ProductImage>> GetAllByProductIdAsync(int  productId);
        Task DeleteAsync(ProductImage productImage);
    }
}
