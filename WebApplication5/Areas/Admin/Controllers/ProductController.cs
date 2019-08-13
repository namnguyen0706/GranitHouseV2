using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Data;
using WebApplication5.Models.ViewModels;
using WebApplication5.Ultility;

namespace WebApplication5.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly HostingEnvironment _hostingEnviroment;

        [BindProperty]
        public ProductViewModel ProductVM { get; set; }

        public ProductController(ApplicationDbContext db, HostingEnvironment hostingEnviroment)
        {
            _db = db;
            _hostingEnviroment = hostingEnviroment;
            ProductVM = new ProductViewModel()
            {
                ProductTypes = _db.ProductTypes.ToList(),
                SpecialTags = _db.SpecialTags.ToList(),
                Product = new Models.Product()
            };
        }

        public async Task<IActionResult> Index()
        {
            var product = _db.Products.Include(m => m.ProductType).Include(m => m.SpecialTag);
            return View(await product.ToListAsync());
        }

        public IActionResult Create()
        {
            return View(ProductVM);
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            if (!ModelState.IsValid)
            {
                return View(ProductVM);
            }

            _db.Products.Add(ProductVM.Product);
            await _db.SaveChangesAsync();

            //Image saved
            string webRootPath = _hostingEnviroment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var productsFromDb = _db.Products.Find(ProductVM.Product.Id);

            if (files.Count != 0)
            {
                // Image has been upload
                var upload = Path.Combine(webRootPath, SD.ImageFolder);
                var extension = Path.GetExtension(files[0].FileName);

                using (var filestream = new FileStream(Path.Combine(upload, ProductVM.Product.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }
                productsFromDb.Image = @"\" + SD.ImageFolder + @"\" + ProductVM.Product.Id + extension;
            }
            else
            {
                // when user does not upload image
                var uploads = Path.Combine(webRootPath, SD.ImageFolder + @"\" + SD.DefaultProductImage);
                System.IO.File.Copy(uploads, webRootPath + @"\" + SD.ImageFolder + @"\" + ProductVM.Product.Id + ".png");
                productsFromDb.Image = @"\" + SD.ImageFolder + @"\" + ProductVM.Product.Id + ".png";
            }
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}