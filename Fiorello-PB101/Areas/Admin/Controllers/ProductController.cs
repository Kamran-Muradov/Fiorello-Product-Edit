using Fiorello_PB101.Helpers;
using Fiorello_PB101.Helpers.Extensions;
using Fiorello_PB101.Models;
using Fiorello_PB101.Services.Interfaces;
using Fiorello_PB101.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;

namespace Fiorello_PB101.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IProductImageService _productImageService;
        private readonly IWebHostEnvironment _env;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            IProductImageService productImageService,
            IWebHostEnvironment env)
        {
            _productService = productService;
            _categoryService = categoryService;
            _productImageService = productImageService;
            _env = env;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var products = await _productService.GetAllPaginateAsync(page, 4);

            var mappedDatas = _productService.GetMappedDatas(products);

            int totalPage = await GetPageCountAsync(4);

            Paginate<ProductVM> paginateDatas = new(mappedDatas, totalPage, page);

            return View(paginateDatas);
        }

        private async Task<int> GetPageCountAsync(int take)
        {
            int productCount = await _productService.GetCountAsync();

            return (int)Math.Ceiling((decimal)productCount / take);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id is null) return BadRequest();

            var existProduct = await _productService.GetByIdWithAllDatasAsync((int)id);

            if (existProduct is null) return NotFound();

            List<ProductImageVM> images = existProduct.ProductImages
                .Select(m => new ProductImageVM
                {
                    Image = m.Name,
                    IsMain = m.IsMain
                })
                .ToList();

            ProductDetailVM response = new()
            {
                Name = existProduct.Name,
                Description = existProduct.Description,
                Category = existProduct.Category.Name,
                Price = existProduct.Price,
                Images = images
            };

            return View(response);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.categories = await _categoryService.GetAllSelectedAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateVM request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.categories = await _categoryService.GetAllSelectedAsync();
                return View();
            }

            foreach (var item in request.Images)
            {
                if (!item.CheckFileSize(500))
                {
                    ModelState.AddModelError("Images", "Image size can be max 500 Kb");
                    ViewBag.categories = await _categoryService.GetAllSelectedAsync();
                    return View();
                }

                if (!item.CheckFileType("image/"))
                {
                    ModelState.AddModelError("Images", "File type must be only image");
                    ViewBag.categories = await _categoryService.GetAllSelectedAsync();
                    return View();
                }
            }

            List<ProductImage> images = new();

            foreach (var item in request.Images)
            {
                string fileName = $"{Guid.NewGuid()}-{item.FileName}";

                string path = _env.GenerateFilePath("img", fileName);

                await item.SaveFileToLocalAsync(path);

                images.Add(new ProductImage { Name = fileName });
            }

            images.FirstOrDefault().IsMain = true;

            Product product = new()
            {
                Name = request.Name,
                Description = request.Description,
                CategoryId = request.CategoryId,
                Price = decimal.Parse(request.Price.Replace(".", ",")),
                ProductImages = images
            };

            await _productService.CreateAsync(product);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();

            var existProduct = await _productService.GetByIdWithAllDatasAsync((int)id);

            if (existProduct is null) return NotFound();

            foreach (var item in existProduct.ProductImages)
            {
                string path = _env.GenerateFilePath("img", item.Name);

                path.DeleteFileFromLocal();
            }

            await _productService.DeleteAsync(existProduct);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return BadRequest();

            var existProduct = await _productService.GetByIdWithAllDatasAsync((int)id);

            if (existProduct is null) return NotFound();

            List<ProductImageEditVM> images = existProduct.ProductImages
                .Select(m => new ProductImageEditVM
                {
                    Id = m.Id,
                    Image = m.Name,
                    IsMain = m.IsMain,
                    ProductId = m.ProductId
                })
                .ToList();

            ProductEditVM response = new()
            {
                Name = existProduct.Name,
                Description = existProduct.Description,
                Price = existProduct.Price.ToString(),
                Images = images
            };

            ViewBag.categories = await _categoryService.GetAllSelectedAsync();

            return View(response);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, ProductEditVM request)
        {
            if (id is null) return BadRequest();

            var existProduct = await _productService.GetByIdWithAllDatasAsync((int)id);

            if (existProduct is null) return NotFound();

            List<ProductImageEditVM> images = existProduct.ProductImages
                .Select(m => new ProductImageEditVM
                {
                    Id = m.Id,
                    Image = m.Name,
                    IsMain = m.IsMain
                })
                .ToList();

            request.Images = images;

            if (!ModelState.IsValid)
            {
                ViewBag.categories = await _categoryService.GetAllSelectedAsync();
                return View(request);
            }

            if (request.NewImages is not null)
            {
                foreach (var item in request.NewImages)
                {
                    if (!item.CheckFileSize(500))
                    {
                        ModelState.AddModelError("Images", "Image size can be max 500 Kb");
                        ViewBag.categories = await _categoryService.GetAllSelectedAsync();
                        return View(request);
                    }

                    if (!item.CheckFileType("image/"))
                    {
                        ModelState.AddModelError("Images", "File type must be only image");
                        ViewBag.categories = await _categoryService.GetAllSelectedAsync();
                        return View(request);
                    }
                }
            }

            await _productService.EditAsync((int)id, request);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProductImage(int? id)
        {
            if (id is null) return BadRequest();

            var productImage = await _productImageService.GetByIdAsync((int)id);

            if (productImage is null) return NotFound();

            if (!productImage.IsMain)
            {
                string path = _env.GenerateFilePath("img", productImage.Name);
                path.DeleteFileFromLocal();
                await _productImageService.DeleteAsync(productImage);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SetMainImage(int? imgId, int? productId)
        {
            if (imgId is null || productId is null) return BadRequest();

            await _productService.SetMainImageAsync((int)imgId, (int)productId);

            return RedirectToAction(nameof(Index));
        }
    }
}
