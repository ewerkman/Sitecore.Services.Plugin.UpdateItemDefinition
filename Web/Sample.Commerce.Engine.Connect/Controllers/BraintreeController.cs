using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Sitecore.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Engine.Connect.Pipelines.Arguments;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Services.Carts;
using Sitecore.Commerce.Services.Orders;
using Sitecore.Commerce.Services.Payments;
using Sitecore.Diagnostics;

namespace Sample.Commerce.Engine.Connect.Controllers
{
    public class BraintreeController : Controller
    {
        private OrderServiceProvider _orderServiceProvider;
        private CartServiceProvider _cartServiceProvider;

        public BraintreeController()
        {
            _cartServiceProvider = new CartServiceProvider();
            _orderServiceProvider = new OrderServiceProvider();
        }

        public ActionResult Index()
        {
            var paymentServiceProvider = new PaymentServiceProvider();

            var request = new ServiceProviderRequest();
            request.SetShopName("CommerceEngineDefaultStorefront");
            request.CurrencyCode = "EUR";

            var result =
                paymentServiceProvider.RunPipeline<ServiceProviderRequest, PaymentClientTokenResult>(
                    "commerce.payments.getClientToken", request);

            return View(result);
        }

        [HttpPost]
        public ActionResult SubmitPayment(string paymentNonce)
        {
            var loadCartRequest = new LoadCartRequest("CommerceEngineDefaultStorefront", "Cart01", "1234");
            var loadCartResult = _cartServiceProvider.LoadCart(loadCartRequest);
            var cart = loadCartResult.Cart as CommerceCart;
            
                        // Add a shipping address
            CommerceParty shippingAddress = new CommerceParty();
            shippingAddress.ExternalId = "Shipping";
            shippingAddress.PartyId = shippingAddress.ExternalId;
            shippingAddress.Name = "Shipping";
            shippingAddress.Address1 = "Barbara Strozzilaan 201";
            shippingAddress.Company = "Sitecore";
            shippingAddress.Country = "Canada";
            shippingAddress.State = "NB"; // State is checked by commerce engine: you can configure it in Commerce 
            shippingAddress.CountryCode = "CA"; // Country is checked by commerce engine
            shippingAddress.LastName = "Werkman";
            shippingAddress.FirstName = "Erwin";
            shippingAddress.City = "Amsterdam";
            shippingAddress.ZipPostalCode = "1030AC";

            var cartParties = cart.Parties.ToList();
            cartParties.Add(shippingAddress);
            cart.Parties = cartParties;

            ShippingOptionType shippingOptionType = ShippingOptionType.ShipToAddress;

            ICollection<CommerceShippingInfo> shippingInfoList = new List<CommerceShippingInfo>();

            var commerceShippingInfo = new CommerceShippingInfo();

            commerceShippingInfo.ShippingOptionType = ShippingOptionType.ShipToAddress;
            commerceShippingInfo.PartyID = shippingAddress.ExternalId;
            commerceShippingInfo.ShippingMethodID = "B146622D-DC86-48A3-B72A-05EE8FFD187A"; // Ship Items > Ground
            commerceShippingInfo.ShippingMethodName =
                "Ground"; // Method id and name have to match what is configured in Sitecore Commerce Control Panel

            shippingInfoList.Add(commerceShippingInfo);

            var csShippingInfoList = new List<ShippingInfo>();

            foreach (var shippingInfo in shippingInfoList)
            {
                csShippingInfoList.Add(shippingInfo);
            }

            // Add a shipping address and shipping method
            var addShippingInfoRequest =
                new Sitecore.Commerce.Engine.Connect.Services.Carts.AddShippingInfoRequest(cart, csShippingInfoList,
                    shippingOptionType);
            var result = _cartServiceProvider.AddShippingInfo(addShippingInfoRequest);

            Assert.IsTrue(result.Success, String.Join("|", result.SystemMessages.Select(e => e.Message)));

            // Reload the cart so we have the latest information on how much we need to pay
            var reloadCartRequest = new LoadCartRequest("CommerceEngineDefaultStorefront", "Cart01", "1234");
            var reloadedCartResult = _cartServiceProvider.LoadCart(reloadCartRequest);

            Assert.IsTrue(reloadedCartResult.Success,
                String.Join("|", reloadedCartResult.SystemMessages.Select(e => e.Message)));

            cart = reloadedCartResult.Cart as CommerceCart;

            CommerceParty billingAddress = new CommerceParty();
            billingAddress.ExternalId = "Billing";      // This should correspond to the PartyId you are setting for the payment info
            billingAddress.PartyId = billingAddress.ExternalId;
            billingAddress.Name = "Billing";
            billingAddress.Address1 = "Dorpsstraat 50";
            billingAddress.Company = "Sitecore";
            billingAddress.Country = "Canada";
            billingAddress.State = "NB";                   // State is checked: you can configure it in Commerce 
            billingAddress.CountryCode = "CA";
            billingAddress.LastName = "Werkman";
            billingAddress.FirstName = "Erwin";
            billingAddress.City = "Amsterdam";
            billingAddress.ZipPostalCode = "1234AK";
            
            cart.Parties.Add(billingAddress);

            var payments = new List<PaymentInfo>();

            var federatedPaymentInfo = new FederatedPaymentInfo();

            federatedPaymentInfo.PartyID = billingAddress.ExternalId;
            federatedPaymentInfo.PaymentMethodID = "0CFFAB11-2674-4A18-AB04-228B1F8A1DEC"; // Federated Payment
            federatedPaymentInfo.Amount = cart.Total.Amount; // Total payment (of all payment methods for a cart) should always be the same as the total amount of the order
            federatedPaymentInfo.CardToken = paymentNonce;

            payments.Add(federatedPaymentInfo);

            var addPaymentInfoRequest = new AddPaymentInfoRequest(cart, payments);
            var addPaymentInfoResult = _cartServiceProvider.AddPaymentInfo(addPaymentInfoRequest);

            Assert.IsTrue(addPaymentInfoResult.Success,
                String.Join("|", addPaymentInfoResult.SystemMessages.Select(e => e.Message)));
            
            cart.Email = "erwin.werkman@sitecore.com"; // This is necessary otherwise the cart will not become an order

            // Save the cart as an order
            var submitVisitorOrderRequest = new SubmitVisitorOrderRequest(cart);
            var submitVisitorOrderResult = _orderServiceProvider.SubmitVisitorOrder(submitVisitorOrderRequest);

            Assert.IsTrue(submitVisitorOrderResult.Success,
                String.Join("|", submitVisitorOrderResult.SystemMessages.Select(e => e.Message)));

            return View(submitVisitorOrderResult);
        }
    }
}