using CoreCrudWithJwt.Data;
using CoreCrudWithJwt.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreCrudWithJwt.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }
        public IActionResult Index()
        {
            var products = _context.Products.Select(x => new ProductDto { Id = x.Id, ProductName = x.ProductName, Description = x.Description, Price = x.Price }).ToList();
            return View(products);
        }

        public async Task<IActionResult> CreateProduct(ProductDto productDto)
        {
            var ExistingProduct = _context.Products.FirstOrDefault(x => x.ProductName == productDto.ProductName);
            if (ExistingProduct == null)
            {
                if (productDto == null || productDto.ProductName == null || productDto.Description == null || productDto.Price == 0)
                {
                    return View("CreateProduct");
                }
                else
                {
                    var products = new Models.Product
                    {
                        ProductName = productDto.ProductName,
                        Description = productDto.Description,
                        Price = productDto.Price
                    };
                    _context.Products.Add(products);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Product Already There";
                return RedirectToAction("Index");
            }
            TempData["SuccessMessage"] = "Success, check dashboard";
            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> EditProduct(int id, string ProductName, string Description, decimal Price)
        {
            var ExistingProduct = _context.Products.FirstOrDefault(x => x.Id == id);
            if (ExistingProduct != null)
            {
                ExistingProduct.ProductName = ProductName;
                ExistingProduct.Description = Description;
                ExistingProduct.Price = Price;

                _context.Products.Update(ExistingProduct);
                await _context.SaveChangesAsync();
            }
            else
            {
                ViewBag.ErrorMessage = "Something went wrong";
                return RedirectToAction("Index");
            }
            TempData["SuccessMessage"] = "Success, check dashboard";
            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> DeleteProduct(int id)
        {
            var ExistingProduct = _context.Products.FirstOrDefault(x => x.Id == id);
            if (ExistingProduct != null)
            {
                _context.Products.Remove(ExistingProduct);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Success, check dashboard";
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ViewBag.ErrorMessage = "Something went wrong";
                return RedirectToAction("Index");
            }
        }
        public IActionResult ProductForm() => View();

        public IActionResult UpdateproductForm(int id)
        {
            var data = _context.Products.Select(x => new ProductDto { Id = x.Id, ProductName = x.ProductName }).FirstOrDefault();
            return View(data);
        }
    }
}
