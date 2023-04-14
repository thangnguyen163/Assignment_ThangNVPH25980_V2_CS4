using Assignment_ThangNVPH25980.IServices;
using Assignment_ThangNVPH25980.Models;
using Assignment_ThangNVPH25980.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Assignment_ThangNVPH25980.Views
{
    public class ManagementController : Controller
    {
        private readonly IProductServices productServices;
        private readonly OuteDBContext context;
        private readonly IColorServices colorServices;
        private readonly ISizeServices sizeServices;
        private readonly ICategoryServices categoryServices;
        private readonly IProductDetailServices productDetailServices;
        public ManagementController()
        {
            productServices=new ProductServices();
            context=new OuteDBContext();
            colorServices=new ColorServices();
            sizeServices=new SizeServices();    
            categoryServices=new CategoryServices();    
            productDetailServices=new ProductDetailServices();
        }
        public IActionResult Index()
        {
            return View();
        } 
        public IActionResult ProductList()
        {
            List<Products> products = productServices.GetAllProducts();
            ViewBag.Products = products;
            var lst = context.Products.Include("Category").ToList();
            return View(lst);
            
        }
        public IActionResult Details(Guid Id)
        {
            OuteDBContext context = new OuteDBContext();
            var product = context.Products.Include("Size").Include("Color").Include("Category").FirstOrDefault(x => x.Id == Id);
            return View(product);
        }
        #region AddProduct
        public IActionResult AddProduct()
        {
            List<Color> colors = colorServices.GetAllColor();
            List<Size> sizes = sizeServices.GetAllSize();
            List<Category> categories = categoryServices.GetAllCategory();
            ViewBag.Colors = colors;
            ViewBag.Sizes = sizes;
            ViewBag.Categories = categories;
            return View();
        }
        //public IActionResult DetailProduct(Guid Id)
        //{
        //    var product = context.Products.Find(Id);
        //    return View(product);
        //}

        [HttpPost]
        public IActionResult AddProduct(Products products, [Bind] IFormFile ImageFile)
        {
            var x = ImageFile.FileName;
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", ImageFile.FileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }
            }
            products.Image = ImageFile.FileName;
            if (productServices.Add(products))
                return RedirectToAction("ProductList");
            else return BadRequest();
        }
        #endregion

        [HttpGet]
        public IActionResult EditProduct(Guid Id)
        {
            List<Color> colors = colorServices.GetAllColor();
            List<Size> sizes = sizeServices.GetAllSize();
            List<Category> categories = categoryServices.GetAllCategory();
            ViewBag.Colors = colors;
            ViewBag.Sizes = sizes;
            ViewBag.Categories = categories;
            var product = productServices.GetProductsById(Id);
            return View(product);
        }

        public IActionResult EditProduct(Products p, [Bind] IFormFile ImageFile)
        {
            var x = ImageFile.FileName;
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", ImageFile.FileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }
            }
            p.Image = ImageFile.FileName;
            if (productServices.Update(p))
                return RedirectToAction("ProductList");
            else return BadRequest();
        }
        [HttpGet]
        public IActionResult ShowListProdcutDetail(Guid Id)
        {
            List<ProductDetails> products = context.ProductDetails.Where(x=>x.ProductId==Id).Include("Color").Include("Size").Include("Products").ToList();
            ViewBag.ProductDetails=products;
            return View();
        }
        //
        //public IActionResult ShowListProdcutDetail(Guid Id)
        //{

        //}

        [HttpGet]
        public IActionResult AddProductDetail(Guid Id)
        {
            List<Size> sizes = sizeServices.GetAllSize();
            List<Color> colors=colorServices.GetAllColor();
            ViewBag.Sizes=sizes;
            ViewBag.Color=colors;
            var p=productServices.GetProductsById(Id);
            return View(p);
        }
        [HttpPost]
        public IActionResult AddProductDetail(ProductDetails productDetails,Guid Id)
        {
            productDetails.ProductId= Id;
            if (productDetailServices.Add(productDetails))
            {
                return RedirectToAction("AddProduct");
            }
            return View();
        }
        [HttpGet]
        public IActionResult UpdateProductDetail(Guid Id)
        {
            var p = productDetailServices.GetProductDetailsById(Id);
            return View(p);
        }
        public IActionResult UpdateProductDetail(ProductDetails productDetails)          
        {
            if (productDetailServices.Update(productDetails))
            {
                return RedirectToAction("ShowListProdcutDetail");
            }
            return View();
        }

        public IActionResult DeleteProduct(Guid Id)

        {   if (productServices.Delete(Id))
            {
                return RedirectToAction("ProductList");
            }
            return View();
        }

        public IActionResult DeleteTest(Guid Id)
        {
            Products a = productServices.GetProductsById(Id);
            HttpContext.Session.SetString("delete", JsonConvert.SerializeObject(a));
            if (productServices.Delete(Id))
            {
                return RedirectToAction("ProductList");
            }
            return View();
        }
        public IActionResult RollBack()
        {
            string json = HttpContext.Session.GetString("delete");
            var pro=JsonConvert.DeserializeObject<Products>(json);
            if (productServices.Add(pro))
            {
                return RedirectToAction("ProductList");
            }
            return View();
        }

    }
}
