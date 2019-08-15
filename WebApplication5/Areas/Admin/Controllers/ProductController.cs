using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Data;
using WebApplication5.Models;
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductVM.Product = await _db.Products.Include(m => m.ProductType).Include(m => m.SpecialTag).SingleOrDefaultAsync(m => m.Id == id);

            if (ProductVM.Product == null)
            {
                return NotFound();
            }

            return View(ProductVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostingEnviroment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                var productFromDb = _db.Products.Where(m => m.Id == ProductVM.Product.Id).FirstOrDefault();

                if (files.Count > 0 && files[0] != null)

                {
                    // if user upload image
                    var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                    var extension_new = Path.GetExtension(files[0].FileName);
                    var extension_old = Path.GetExtension(productFromDb.Image);

                    if (System.IO.File.Exists(Path.Combine(uploads, ProductVM.Product.Id + extension_old)))
                    {
                        System.IO.File.Delete(Path.Combine(uploads, ProductVM.Product.Id + extension_old));
                    }
                    using (var filestream = new FileStream(Path.Combine(uploads, ProductVM.Product.Id + extension_new), FileMode.Create))
                    {
                        files[0].CopyTo(filestream);
                    }
                    ProductVM.Product.Image = @"\" + SD.ImageFolder + @"\" + ProductVM.Product.Id + extension_new;
                }

                if (ProductVM.Product.Image != null)
                {
                    productFromDb.Image = ProductVM.Product.Image;
                }

                productFromDb.Name = ProductVM.Product.Name;
                productFromDb.Price = ProductVM.Product.Price;
                productFromDb.Avaiable = ProductVM.Product.Avaiable;
                productFromDb.ProductTypeId = ProductVM.Product.ProductTypeId;
                productFromDb.SpecialTagId = ProductVM.Product.SpecialTagId;
                productFromDb.ShadeColor = ProductVM.Product.ShadeColor;

                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

            }
            return View(ProductVM);
        }


        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductVM.Product = await _db.Products.Include(m =>m.ProductType).Include(m => m.SpecialTag).SingleOrDefaultAsync(m => m.Id == id);

            if (ProductVM.Product == null)
            {
                return NotFound();
            }

            return View(ProductVM);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductVM.Product = await _db.Products.Include(m => m.ProductType).Include(m => m.SpecialTag).SingleOrDefaultAsync(m => m.Id == id);

            if (ProductVM.Product == null)
            {
                return NotFound();
            }

            return View(ProductVM);
        }


        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string webRootPath = _hostingEnviroment.WebRootPath;
            Product products = await _db.Products.FindAsync(id);

            if (products == null)
            {
                return NotFound();
            }
            else
            {
                var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                var extension = Path.GetExtension(products.Image);

                if (System.IO.File.Exists(Path.Combine(uploads,products.Id +extension)))
                {
                    System.IO.File.Delete(Path.Combine(uploads, products.Id + extension));
                }

                _db.Products.Remove(products);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
        }
    }
}