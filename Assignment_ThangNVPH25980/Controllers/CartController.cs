using Assignment_ThangNVPH25980.IServices;
using Assignment_ThangNVPH25980.Models;
using Assignment_ThangNVPH25980.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Assignment_ThangNVPH25980.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartDetailServices cartDetailServices;
        public CartController()
        {
            cartDetailServices = new CartDetailServices();
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Cart()
        {
            List<CartDetails> cartDetails = cartDetailServices.GetAllCartDetails();
            ViewBag.CartDetail = cartDetails;
            return View();
        }
        [HttpPost]
        public IActionResult Cart(Guid IdPro)
        {

            string user = HttpContext.Session.GetString("User");
            Accounts users = JsonConvert.DeserializeObject<Accounts>(user);
            List<CartDetails> cartDetails = cartDetailServices.GetAllCartDetails().Where(x => x.UserId == users.Id).ToList();

            // List<Cart> Carts =_cartService.GetAllCarts().Where(x => x.UserId == users.Id).ToList();
            if (cartDetails.Count == 0)
            {
                CartDetails cartDetail = new CartDetails()
                {
                    Id = Guid.NewGuid(),
                    UserId = users.Id,
                    ProductId = IdPro,
                    Quantity = 1,
                };
                cartDetailServices.Add(cartDetail);
            }
            else 
            {
                CartDetails cartDetail = new CartDetails();
                cartDetail = cartDetailServices.GetAllCartDetails().FirstOrDefault(x => x.UserId == users.Id && x.ProductId == IdPro);
                cartDetail.Quantity = cartDetail.Quantity + 1;
                cartDetailServices.Update(cartDetail);
                
            }
            return RedirectToAction("Cart");
        }
    }
}
