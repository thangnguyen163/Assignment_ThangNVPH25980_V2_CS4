using Assignment_ThangNVPH25980.IService;
using Assignment_ThangNVPH25980.IServices;
using Assignment_ThangNVPH25980.Models;
using Assignment_ThangNVPH25980.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Assignment_ThangNVPH25980.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductServices productServices;
        private readonly IColorServices colorServices;
        private readonly ISizeServices sizeServices;
        private readonly ICategoryServices categoryServices;
        private readonly IAccountServices accountServices;
        private readonly ICartDetailServices cartDetailServices;
        private readonly IBillServices billServices;
        private readonly IBillDeatailServices billDeatailServices;
        private readonly OuteDBContext context;
        private readonly IProductDetailServices productDetailServices;


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            productServices = new ProductServices();
            colorServices = new ColorServices();
            sizeServices = new SizeServices();
            categoryServices = new CategoryServices();
            accountServices = new AccountServices();
            cartDetailServices = new CartDetailServices();
            context = new OuteDBContext();
            billDeatailServices = new BillDeatailServices();
            billServices = new BillServices();
            productDetailServices = new ProductDetailServices();

        }

        public IActionResult Index()
        {
            string user = HttpContext.Session.GetString("User");
            var users = JsonConvert.DeserializeObject<Accounts>(user);
            ViewBag.User = users;
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Products()
        {
            List<Products> products = productServices.GetAllProducts();
            //List<ProductView> products = productServices.GetAllProductView();
            ViewBag.Products = products;
            return View();
        }

        [HttpGet]
        public IActionResult Detail(Guid Id)
        {
            OuteDBContext context = new OuteDBContext();
            var product = context.Products.Where(x => x.Id == Id).FirstOrDefault();
            var product1 = context.ProductDetails.Where(x => x.Id == Id).ToList();
            List<Color> colors = colorServices.GetAllColor();
            List<Size> sizes = sizeServices.GetAllSize();
            List<Category> categories = categoryServices.GetAllCategory();

            for (int i = 0; i < product1.Count(); i++)
            {
                colors = context.Color.Where(x => x.Id == product1[i].ColorId).ToList();
                sizes = context.Size.Where(x => x.Id == product1[i].SizeId).ToList();
            }
            ViewBag.Colors = colors;
            ViewBag.Sizes = sizes;
            ViewBag.Categories = categories;
            return View(product);


        }
        #region Bill PayToBill
        public IActionResult Bill()
        {
            List<BillDetails> billDetails = billDeatailServices.GetAllBillDetail();
            List<Bill> bills = context.Bill.Include("Accounts").Include("BillDetails").ToList();
            ViewBag.BillDetails = billDetails;
            ViewBag.Bills = bills;
            return View();
        }
        [HttpPost]
        public IActionResult PayToBill()
        {
            string user = HttpContext.Session.GetString("User");
            if (user == null)
            {
                return RedirectToAction("Login");

            }
            Accounts users = JsonConvert.DeserializeObject<Accounts>(user);

            var list = cartDetailServices.GetAllCartDetails().Where(x => x.UserId == users.Id);
            //Nếu giỏ hàng chưa có đồ thì bắt mua
            if (list.Count() == 0)
            {
                return RedirectToAction("Products");
            }
            else
            {
                Guid id = Guid.NewGuid();
                //Nếu giỏ hàng có đồ thì tạo hóa đơn, hóa đơn chi tiết và xóa dữ liệu trong giỏ hàng 
                Bill bill = new Bill()
                {
                    Id = id,
                    CreatedDate = DateTime.Now,
                    UserId = users.Id,
                    Status = 1,
                };
                billServices.Add(bill);
                var listCartDetail = cartDetailServices.GetAllCartDetails().Where(x => x.UserId == users.Id).ToList(); //danh sách giỏ hàng
                for (int i = 0; i < listCartDetail.Count(); i++)
                {
                    var productDetails = productDetailServices.GetProductDetailsById(listCartDetail[i].ProductId);//lấy ra 1 sản phẩm;
                    var product = productServices.GetAllProducts().Where(x => x.Id == productDetails.ProductId).FirstOrDefault();
                    var cartdetail = cartDetailServices.GetAllCartDetails().FirstOrDefault(x => x.ProductId == productDetails.Id && x.UserId == users.Id);//lấy ra thằng giỏ hàng chi tiết để lấy số lượng
                    BillDetails billDetail = new BillDetails();
                    billDetail.Id = Guid.NewGuid();
                    billDetail.BillId = id;
                    billDetail.ProductId = productDetails.Id;
                    billDetail.Quantity = cartdetail.Quantity;
                    billDetail.Price = cartdetail.Quantity * product.Price;
                    billDeatailServices.Add(billDetail);
                    //Add 1 sản phẩm vào bill detail
                    //Sau đó Update số lượng
                    //Product.AvailableQuantity = Product.AvailableQuantity - cartdetail.Quantity;
                    productDetailServices.Update(productDetails);
                    //Xóa ở CartDetail
                    cartDetailServices.Delete(cartdetail.Id);
                }
                return RedirectToAction("Bill");
            }

        }
        #endregion


        public IActionResult BillDetail(Guid billId)
        {

            List<BillDetails> billDetails = context.BillDetails.Include("ProductDetails").Where(x => x.BillId == billId).ToList();
            ViewBag.BillDetail = billDetails;

            List<ProductDetails> productDetails = context.ProductDetails.Include("Size").Include("Color").Include("Products").ToList();
            ViewBag.Products = productDetails;
            return View();
        }
        [HttpGet]
        public IActionResult UpdateBillDetail(Guid Id)
        {
            var a = billServices.GetBillById(Id);
            return View(a);
        }

        public IActionResult UpdateBillDetail(Bill bill)
        {
            if (billServices.Update(bill))
            {
                return RedirectToAction("Bill");
            }
            return View();
        }



        #region AddToCart   Cart
        public IActionResult Cart()
        {
            string user = HttpContext.Session.GetString("User");
            Accounts users = JsonConvert.DeserializeObject<Accounts>(user);

            List<CartDetails> cartDetails = context.CartDetails.Include("ProductDetails").Where(x => x.UserId == users.Id).ToList();
            List<ProductDetails> productDetails = context.ProductDetails.Include("Size").Include("Color").Include("Products").ToList();
            List<Products> products1 = productServices.GetAllProducts();
            ViewBag.CartDetail = cartDetails;
            ViewBag.ProductDe = productDetails;
            ViewBag.Products = products1;
            return View();
        }
        [HttpPost]
        public IActionResult Cart(Guid IdPro, Guid ColorId, Guid SizeId, int quantity)
        {

            string user = HttpContext.Session.GetString("User");



            if (user == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                Accounts users = JsonConvert.DeserializeObject<Accounts>(user);
                List<CartDetails> cartDetails = cartDetailServices.GetAllCartDetails().Where(x => x.UserId == users.Id && x.ProductId == IdPro).ToList();
                ProductDetails a = productDetailServices.GetAllProductDetails().FirstOrDefault(x => x.ProductId == IdPro && x.ColorId == ColorId && x.SizeId == SizeId);
                if (a == null)
                {
                    return Content("Sản phẩm có vấn đề ");
                }
                else
                {
                    List<CartDetails> cartDetail1 = cartDetailServices.GetAllCartDetails().Where(x => x.UserId == users.Id && x.ProductId == a.Id).ToList();
                    // List<Cart> Carts =_cartService.GetAllCarts().Where(x => x.UserId == users.Id).ToList();
                    if (cartDetail1.Count == 0)
                    {
                        CartDetails cartDetail = new CartDetails()
                        {
                            Id = Guid.NewGuid(),
                            UserId = users.Id,
                            ProductId = a.Id,
                            Quantity = quantity,
                        };
                        cartDetailServices.Add(cartDetail);

                    }
                    else
                    {
                        CartDetails cartDetail = new CartDetails();
                        cartDetail = cartDetailServices.GetAllCartDetails().FirstOrDefault(x => x.UserId == users.Id && x.ProductId == a.Id);
                        cartDetail.Quantity = cartDetail.Quantity + quantity;
                        cartDetailServices.Update(cartDetail);

                    }

                }
                return RedirectToAction("Cart");
                //return View();
            }
        }
        public IActionResult Delete(Guid Id)
        {
            if (cartDetailServices.Delete(Id))
                return RedirectToAction("Cart");
            return BadRequest();
        }



        #endregion


        #region Login + SignUp
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string user, string pass)
        {
            Accounts a = accountServices.GetAllAccounts().FirstOrDefault(x => x.UserName == user && x.Password == pass);
            HttpContext.Session.SetString("User", JsonConvert.SerializeObject(a));
            if (a != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SignUp(Accounts accounts)
        {
            if (accountServices.Add(accounts))
                return RedirectToAction("Login");
            else return BadRequest();
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("User");
            return RedirectToAction("Login");
        }
        #endregion


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}