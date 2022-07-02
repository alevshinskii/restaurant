using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class ProductController : Controller
    {
        private readonly RestaurantContext db;
        IWebHostEnvironment env;

        public ProductController(RestaurantContext db,IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }
        
        public async Task<IActionResult> Index(SortOrder sortOrder=SortOrder.NameAsc)
        {
            IQueryable<Product> products = db.Products.Include(x => x.Category);

            ViewData["NameSort"] = sortOrder == SortOrder.NameAsc ? SortOrder.NameDesc : SortOrder.NameAsc;
            ViewData["PriceSort"] = sortOrder == SortOrder.PriceAsc ? SortOrder.PriceDesc : SortOrder.PriceAsc;
            ViewData["AvailableSort"] = sortOrder == SortOrder.AvailableAsc ? SortOrder.AvailableDesc : SortOrder.AvailableAsc;
            ViewData["CategorySort"] = sortOrder == SortOrder.CategoryAsc ? SortOrder.CategoryDesc : SortOrder.CategoryAsc;

            products = sortOrder switch
            {
                SortOrder.NameDesc => products.OrderByDescending(p => p.Name),
                SortOrder.NameAsc => products.OrderBy(p => p.Name),
                SortOrder.PriceDesc => products.OrderByDescending(p => p.Price),
                SortOrder.PriceAsc => products.OrderBy(p => p.Price),
                SortOrder.AvailableDesc => products.OrderByDescending(p => p.Available),
                SortOrder.AvailableAsc => products.OrderBy(p => p.Available),
                SortOrder.CategoryDesc => products.OrderByDescending(p => p.Category.Name),
                SortOrder.CategoryAsc => products.OrderBy(p => p.Category.Name),
                _ => products.OrderBy(p => p.Name),
            };
            return View(await products.AsNoTracking().ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product= await db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [Authorize(Roles = "manager,admin")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(db.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Description,CategoryId,Available,ImageUrl")] Product product,
            IFormFile upload)
        {
            if (upload != null)
            {
                string fileName = Path.GetFileName(upload.FileName);
                
                if (CheckByGraphicsFormat(fileName))
                {
                    Save(upload, fileName);

                    product.ImageUrl = "/images/products/"+fileName;
                    ModelState.Remove("ImageUrl");
                }
            }
            var category = db.Categories.Find(product.CategoryId);
            if (category != null)
            {
                ModelState.Remove("Category");
                product.Category = category;
            }
            if (ModelState.IsValid)
            {
                db.Add(product);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        private bool CheckByGraphicsFormat(string fileName)
        {
            var ext = fileName.Substring(fileName.Length - 3);
            return string.CompareOrdinal(ext, "png") == 0 || string.CompareOrdinal(ext, "jpg") == 0;
        }

        private void Save(IFormFile upload, string fileName)
        {
            Bitmap image = new Bitmap(upload.OpenReadStream());
            string path = "\\wwwroot\\images\\products\\" + fileName;
            var root = env.ContentRootPath;
            path = root + path;
            // сохраняем файл в папку  в каталоге wwwroot
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                image.Save(fileStream, ImageFormat.Png);
            }
        }

        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Description,CategoryId,Available,ImageUrl")] Product product, IFormFile upload)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (upload != null)
            {
                string fileName = Path.GetFileName(upload.FileName);

                if (CheckByGraphicsFormat(fileName))
                {
                    Save(upload, fileName);
                    product.ImageUrl = "/images/products/"+fileName;
                }
            }
            else 
            { 
                var initialProduct = db.Products.FirstOrDefault(pr => pr.Id == product.Id); 
                db.Entry(initialProduct).State = EntityState.Detached; 
                product.ImageUrl = initialProduct.ImageUrl; 
                db.Entry(product).State = EntityState.Modified; 
            }
            try
            {
                db.Update(product);
                await db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id))
                {
                    ViewData["CategoryId"] = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
                    return View(product);
                }
                else
                {
                    throw;
                }
            }
        }

        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product= await db.Products
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [Authorize(Roles = "manager,admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await db.Products.FindAsync(id);
            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return db.Products.Any(e => e.Id == id);
        }

    }
}

