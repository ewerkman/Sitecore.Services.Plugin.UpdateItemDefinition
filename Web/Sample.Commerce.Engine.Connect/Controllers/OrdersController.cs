using System.Web.Mvc;
using Sitecore.Commerce.Services.Orders;

namespace Sample.Commerce.Engine.Connect.Controllers
{
    public class OrdersController : Controller
    {
        // GET
        public ActionResult Index()
        {
            var orderService = new OrderServiceProvider();

            var visitorOrdersRequest = new GetVisitorOrdersRequest("Entity-Customer-34d758ae2d2d472d89014954d0cc4440", "CommerceEngineDefaultStorefront");

            var result = orderService.GetVisitorOrders(visitorOrdersRequest);
            
            return View(result);
        }

        public ActionResult GetOrder(string orderId)
        {
            var orderService = new OrderServiceProvider();

            var visitorOrderRequest = new GetVisitorOrderRequest(orderId,
                "Entity-Customer-34d758ae2d2d472d89014954d0cc4440", "CommerceEngineDefaultStorefront");
            var result = orderService.GetVisitorOrder(visitorOrderRequest);
            
            return View(result);   
        }
    }
}