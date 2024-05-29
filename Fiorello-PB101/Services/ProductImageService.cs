using Fiorello_PB101.Data;
using Fiorello_PB101.Models;
using Fiorello_PB101.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fiorello_PB101.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly AppDbContext _context;

        public ProductImageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductImage> GetByIdAsync(int id)
        {
            return await _context.ProductImages.FindAsync(id);
        }

        public async Task<List<ProductImage>> GetAllByProductIdAsync(int productId)
        {
            return await _context.ProductImages
                .Where(m => m.ProductId == productId)
                .ToListAsync();
        }

        public async Task DeleteAsync(ProductImage productImage)
        {
            _context.Remove(productImage);
            await _context.SaveChangesAsync();
        }
    }
}
