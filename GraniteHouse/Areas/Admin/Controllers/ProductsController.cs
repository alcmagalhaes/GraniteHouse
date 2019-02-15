using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Models;
using GraniteHouse.Models.ViewModels;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteHouse.Areas.Admin.Controllers
{
    [Area("Admin")] 
    public class ProductsController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly HostingEnvironment _henv;

        [BindProperty]
        public ProductsViewModel ProductsVM { get; set; }

        public ProductsController(ApplicationDbContext db, HostingEnvironment henv)
        {
            _db = db;
            _henv = henv;

            ProductsVM = new ProductsViewModel()
            {
                ProductTypes = _db.ProductTypes.ToList(),
                SpecialTags = _db.SpecialTags.ToList(),
                Products = new Models.Products()
            };
        }

        public async Task<IActionResult> Index()
        {
            var products = _db.Products.Include(m => m.ProductTypes).Include(m => m.SpecialTags);
            return View(await products.ToListAsync());
        }

        //GET : Products Create
        public IActionResult Create()
        {
            return View(ProductsVM);
        }

        //POST : Products Create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync()
        {
            if (!ModelState.IsValid)
            {
                return View(ProductsVM);
            }

            _db.Products.Add(ProductsVM.Products);
            await _db.SaveChangesAsync();

            //Image being saved
            string webRootPath = _henv.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var productsFromDb = _db.Products.Find(ProductsVM.Products.Id);

            if (files.Count != 0)
            {
                //Image has been upload
                var uploads = Path.Combine(webRootPath, SD.ImagesFolder);
                var extension = Path.GetExtension(files[0].FileName);

                using (var fileStream = new FileStream(Path.Combine(uploads, ProductsVM.Products.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }

                productsFromDb.Image = @"\" + SD.ImagesFolder + @"\" + ProductsVM.Products.Id + extension;

            }
            else
            {
                //When user does not upload image
                var uploads = Path.Combine(webRootPath, SD.ImagesFolder + @"\" + SD.DefaultProductImage);
                System.IO.File.Copy(uploads, webRootPath + @"\" + SD.ImagesFolder + @"\" + ProductsVM.Products.Id + ".png");
                productsFromDb.Image = @"\" + SD.ImagesFolder + @"\" + ProductsVM.Products.Id + ".png";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        //GET : Products Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductsVM.Products = await _db.Products.Include(m => m.SpecialTags).Include(m => m.ProductTypes).SingleOrDefaultAsync(m => m.Id == id);

            if (ProductsVM.Products == null)
            {
                return NotFound();
            }

            return View(ProductsVM);
        }

        //POST : Product Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {

            if(ModelState.IsValid)
            {
                string webRootPath = _henv.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                var productsFromDb = _db.Products.Where(m => m.Id == ProductsVM.Products.Id).FirstOrDefault();

                if (files.Count > 0 && files[0] != null)
                {
                    //if user uploads a new image
                    var uploads = Path.Combine(webRootPath, SD.ImagesFolder);
                    var extension_new = Path.GetExtension(files[0].FileName);
                    var extension_old = Path.GetExtension(productsFromDb.Image);

                    if (System.IO.File.Exists(Path.Combine(uploads, ProductsVM.Products.Id + extension_old)))
                    {
                        System.IO.File.Delete(Path.Combine(uploads, ProductsVM.Products.Id + extension_old));
                    }

                    using (var fileStream = new FileStream(Path.Combine(uploads, ProductsVM.Products.Id + extension_new), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    ProductsVM.Products.Image = @"\" + SD.ImagesFolder + @"\" + ProductsVM.Products.Id + extension_new;

                    if (ProductsVM.Products.Image != null)
                    {
                        productsFromDb.Image = ProductsVM.Products.Image;
                    }

                    productsFromDb.Name = ProductsVM.Products.Name;
                    productsFromDb.Price = ProductsVM.Products.Price;
                    productsFromDb.Avaiable = ProductsVM.Products.Avaiable;
                    productsFromDb.ProductTypesId = ProductsVM.Products.ProductTypesId;
                    productsFromDb.SpecialTagsId = ProductsVM.Products.SpecialTagsId;
                    productsFromDb.ShadeColor = ProductsVM.Products.ShadeColor;

                    await _db.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));

                }

            }

            return View(ProductsVM);
        }

    }

}
