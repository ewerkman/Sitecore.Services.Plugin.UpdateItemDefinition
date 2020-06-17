using System.Web.Mvc;
using Sample.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Services.Carts;

namespace Sample.Commerce.Engine.Connect.Controllers
{
    public class SampleController : Controller
    {
        private readonly CartServiceProvider _cartServiceProvider;

        public SampleController()
        {
            _cartServiceProvider = new CartServiceProvider();
        }
        
        public ActionResult Index()
        {
            var loadCartRequest = new LoadCartRequest("CommerceEngineDefaultStorefront", "Cart01", "1234");
            var loadCartResult = _cartServiceProvider.LoadCart(loadCartRequest);
            
            return View(loadCartResult.Cart as CustomCart);
        }
    }
}