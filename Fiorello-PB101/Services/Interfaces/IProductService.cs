using Fiorello_PB101.Models;
using Fiorello_PB101.ViewModels.Products;

namespace Fiorello_PB101.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllWithImagesAsync();
        Task<IEnumerable<Product>> GetAllAsync();
        //Task<IEnumerable<Product>> GetAllAddedBasketAsync();
        Task<Product> GetByIdAsync(int id);
        Task<Product> GetByIdWithAllDatasAsync(int id);
        Task<IEnumerable<Product>> GetAllPaginateAsync(int page, int take);
        IEnumerable<ProductVM> GetMappedDatas(IEnumerable<Product> products);
        Task<int> GetCountAsync();
        Task CreateAsync(Product product);
        Task DeleteAsync(Product product);
        Task EditAsync(int id, ProductEditVM request);
        Task SetMainImageAsync(int imgId, int productId);
    }
}
