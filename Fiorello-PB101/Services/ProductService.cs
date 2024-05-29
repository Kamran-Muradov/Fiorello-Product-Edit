using Fiorello_PB101.Data;
using Fiorello_PB101.Helpers.Extensions;
using Fiorello_PB101.Models;
using Fiorello_PB101.Services.Interfaces;
using Fiorello_PB101.ViewModels.Baskets;
using Fiorello_PB101.ViewModels.Products;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Fiorello_PB101.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IProductImageService _productImageService;
        private readonly IHttpContextAccessor _accessor;
        private readonly IWebHostEnvironment _env;


        public ProductService(
            AppDbContext context,
            IProductImageService productImageService,
            IHttpContextAccessor accessor,
            IWebHostEnvironment env
            )
        {
            _context = context;
            _productImageService = productImageService;
            _accessor = accessor;
            _env = env;
        }

        public async Task<IEnumerable<Product>> GetAllWithImagesAsync()
        {
            return await _context.Products
                .Include(m => m.ProductImages)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(m => m.ProductImages)
                .Include(m => m.Category)
                .ToListAsync();
        }

        //public async Task<IEnumerable<Product>> GetAllAddedBasketAsync()
        //{
        //    List<BasketVM> basketDatas = new();
        //    List<Product> result = new();

        //    if (_accessor.HttpContext.Request.Cookies["basket"] is not null)
        //    {
        //        basketDatas = JsonConvert.DeserializeObject<List<BasketVM>>(_accessor.HttpContext.Request.Cookies["basket"]);
        //    }
        //    foreach (var item in basketDatas)
        //    {
        //        result
        //            .Add(await _context.Products
        //            .Where(m => m.Id == item.Id)
        //            .Include(m => m.Category)
        //            .Include(m => m.ProductImages)
        //            .FirstOrDefaultAsync());
        //    }

        //    return result;
        //}

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product> GetByIdWithAllDatasAsync(int id)
        {
            return await _context.Products
                .Where(m => m.Id == id)
                .Include(m => m.Category)
                .Include(m => m.ProductImages)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Product>> GetAllPaginateAsync(int page, int take)
        {
            return await _context.Products
                .OrderByDescending(m => m.Id)
                .Skip((page - 1) * take)
                .Take(take)
                .Include(m => m.ProductImages)
                .Include(m => m.Category)
                .ToListAsync();
        }

        public IEnumerable<ProductVM> GetMappedDatas(IEnumerable<Product> products)
        {
            return products.Select(m => new ProductVM
            {
                Id = m.Id,
                Name = m.Name,
                CategoryName = m.Category.Name,
                Price = m.Price,
                Description = m.Description,
                MainImage = m.ProductImages.FirstOrDefault(i => i.IsMain)?.Name
            });
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Products.CountAsync();
        }

        public async Task CreateAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task EditAsync(int id, ProductEditVM request)
        {
            var existProduct = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);

            if (request.NewImages is not null)
            {
                foreach (var item in request.NewImages)
                {
                    string fileName = $"{Guid.NewGuid()}-{item.FileName}";

                    string path = _env.GenerateFilePath("img", fileName);

                    await item.SaveFileToLocalAsync(path);

                    existProduct.ProductImages.Add(new ProductImage { Name = fileName });
                }
            }

            existProduct.Name = request.Name;
            existProduct.Description = request.Description;
            existProduct.Price = decimal.Parse(request.Price.Replace(".", ","));
            existProduct.CategoryId = request.CategoryId;

            await _context.SaveChangesAsync();
        }

        public async Task SetMainImageAsync(int imgId, int productId)
        {
            var productImages = await _productImageService.GetAllByProductIdAsync(productId);

            productImages.FirstOrDefault(m => m.IsMain).IsMain = false;

            productImages.FirstOrDefault(m => m.Id == imgId).IsMain = true;

            await _context.SaveChangesAsync();
        }
    }
}
