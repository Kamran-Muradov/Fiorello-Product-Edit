using System.ComponentModel.DataAnnotations;
using Fiorello_PB101.Models;

namespace Fiorello_PB101.ViewModels.Products
{
    public class ProductEditVM
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Price { get; set; }
        public int CategoryId { get; set; }
        public List<ProductImageEditVM> Images { get; set; }
        public List<IFormFile> NewImages { get; set; }

    }
}
