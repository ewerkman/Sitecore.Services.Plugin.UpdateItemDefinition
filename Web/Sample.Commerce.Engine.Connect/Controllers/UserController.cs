using System.Web.Mvc;
using System.Web.Security;
using Sitecore.Commerce.Services.Customers;

namespace Sample.Commerce.Engine.Connect.Controllers
{
    public class UserController : Controller
    {
        public ActionResult CreateUser()
        {
            var customerService = new CustomerServiceProvider();

            var request = new CreateUserRequest("CommerceUsers\\john@abczyx.net", "password", "john@abczyx.net", "CommerceEngineDefaultStorefront");

            var result = customerService.CreateUser(request);
            
            return View(result);
        }

        public ActionResult LoginUser()
        {
            FormsAuthentication.SetAuthCookie("CommerceUsers\\john@abczyx.net", true);
            return View();
        }
    }
}