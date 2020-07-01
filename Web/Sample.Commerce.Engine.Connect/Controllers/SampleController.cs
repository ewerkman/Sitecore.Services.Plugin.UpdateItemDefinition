using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;
using Sample.Commerce.Engine.Connect.Entities;
using Sitecore.Analytics;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Engine.Connect;
using Sitecore.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Engine.Connect.Pipelines.Arguments;
using Sitecore.Commerce.Engine.Connect.Services.Carts;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Entities.GiftCards;
using Sitecore.Commerce.Entities.Shipping;
using Sitecore.Commerce.ServiceProxy;
using Sitecore.Commerce.Services.Carts;
using Sitecore.Commerce.Services.Orders;
using Sitecore.Commerce.Services.Shipping;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Eventing;
using Sitecore.Services.Plugin.Sample.Policies;
using GetShippingMethodsRequest = Sitecore.Commerce.Engine.Connect.Services.Shipping.GetShippingMethodsRequest;

namespace Sample.Commerce.Engine.Connect.Controllers
{
    public class SampleController : Controller
    {
        private readonly CartServiceProvider _cartServiceProvider;
        private readonly OrderServiceProvider _orderServiceProvider;

        public SampleController()
        {
            _cartServiceProvider = new CartServiceProvider();
            _orderServiceProvider = new OrderServiceProvider();
        }

        public ActionResult Index()
        {
            var loadCartRequest = new LoadCartRequest("CommerceEngineDefaultStorefront", "Cart01", "1234");
            var loadCartResult = _cartServiceProvider.LoadCart(loadCartRequest);

            return View("Cart", loadCartResult);
        }

        public ActionResult AddProduct()
        {
            var loadCartRequest = new LoadCartRequest("CommerceEngineDefaultStorefront", "Cart01", "1234");
            var loadCartResult = _cartServiceProvider.LoadCart(loadCartRequest);
            var cart = loadCartResult.Cart as CustomCart;

            // Add a cart line: note that adding a line for the same product twice will update the quantity, not add a line: this is configured in commerce engine (look for RollupCartLinesPolicy)
            var lines = new List<CartLine>();
            var cartLine = new CommerceCartLine("Neu", "47838_aus_allen_sternen_liebe_cd", "", 1.0M);
            lines.Add(cartLine);

            var addLinesRequest = new AddCartLinesRequest(cart, lines);
            var addLinesResult = _cartServiceProvider.AddCartLines(addLinesRequest);

            return View("Cart", addLinesResult);
        }

        public ActionResult GetCartLineFulfillmentMethods()
        {
            // Load a cart
            var loadCartRequest = new LoadCartRequest("CommerceEngineDefaultStorefront", "Cart01", "1234");
            var loadCartResult = _cartServiceProvider.LoadCart(loadCartRequest);
            var cart = loadCartResult.Cart as CustomCart;

            // Add a line to the cart
            var lines = new List<CartLine>();
            var cartLine = new CommerceCartLine("NEU", "test", "", 1.0M);
            lines.Add(cartLine);

            var addLinesRequest = new AddCartLinesRequest(cart, lines);
            var addLinesResult = _cartServiceProvider.AddCartLines(addLinesRequest);

            cart = addLinesResult.Cart as CustomCart;

            var shippingService = new ShippingServiceProvider();

            var shippingOption = new ShippingOption
            {
                ShippingOptionType =
                    ShippingOptionType
                        .DeliverItemsIndividually, // This will trigger calling GetCartLinesFulfillmentMethods instead of GetCartFulfillmentMethods
            };

            var shippingParty = new CommerceParty
            {
                Address1 = "Main Street", City = "Montreal", ZipPostalCode = "NW7 7SJ", Country = "Canada",
                CountryCode = "CA"
            };

            var request = new GetShippingMethodsRequest(shippingOption, shippingParty, cart)
            {
                Lines = cart.Lines.Cast<CommerceCartLine>().ToList()
            };
            var result = shippingService.GetShippingMethods(request);

            return View(result);
        }

        public ActionResult CreateBasket()
        {
            var shopName =
                "CommerceEngineDefaultStorefront"; // Better use a configured store, not CommerceEngineDefaultStorefront as it's not really configured
            var cartName = "Cart01";
            var userId = "1234";

            // Get a cart
            var loadCartRequest = new LoadCartByNameRequest(shopName, cartName, userId);
            var cartResult = _cartServiceProvider.LoadCart(loadCartRequest);

            Assert.IsTrue(cartResult.Success, String.Join("|", cartResult.SystemMessages.Select(e => e.Message)));

            var cart = cartResult.Cart;

            var quantity = 0.0M; // Get initial quantity
            if (cart.Lines.Count > 0)
            {
                quantity = cart.Lines[0].Quantity;
            }

            // Add a cart line: note that adding a line for the same product twice will update the quantity, not add a line: this is configured in commerce engine (look for RollupCartLinesPolicy)
            var lines = new List<CartLine>();
            var cartLine = new CommerceCartLine("Neu", "47838_aus_allen_sternen_liebe_cd", "", 1.0M);
            lines.Add(cartLine);

            var addLinesRequest = new AddCartLinesRequest(cart, lines);
            var addLinesResult = _cartServiceProvider.AddCartLines(addLinesRequest);

            Assert.IsTrue(addLinesResult.Success,
                String.Join("|", addLinesResult.SystemMessages.Select(e => e.Message)));

            var updatedCart = addLinesResult.Cart;

            Assert.IsTrue(updatedCart.Lines.Count > 0, "Updated cart should have at least one line");
            Assert.IsTrue(quantity + 1 == updatedCart.Lines[0].Quantity, "new quantity should be one more than before");

            var reloadCartRequest = new LoadCartByNameRequest(shopName, cartName, userId);
            var reloadedCartResult = _cartServiceProvider.LoadCart(reloadCartRequest);

            Assert.IsTrue(reloadedCartResult.Success,
                String.Join("|", reloadedCartResult.SystemMessages.Select(e => e.Message)));

            var reloadedCart = reloadedCartResult.Cart;

            Assert.IsTrue(reloadedCart.Lines.Count > 0, "Reloaded cart should have at least one line");
            Assert.IsTrue(quantity + 1 == reloadedCart.Lines[0].Quantity,
                "reloaded cart quantity should be one more than before");

            // Switching cart :-)
            cart = reloadedCart;

            // Add a shipping address
            CommerceParty shippingAddress = new CommerceParty();
            shippingAddress.ExternalId = "Shipping";
            shippingAddress.PartyId = shippingAddress.ExternalId;
            shippingAddress.Name = "Shipping";
            shippingAddress.Address1 = "Barbara Strozzilaan 201";
            shippingAddress.Company = "Sitecore";
            shippingAddress.Country = "The Netherlands";
            shippingAddress.State = "ZH"; // State is checked by commerce engine: you can configure it in Commerce 
            shippingAddress.CountryCode = "NL"; // Country is checked by commerce engine
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
            reloadedCartResult = _cartServiceProvider.LoadCart(reloadCartRequest);

            Assert.IsTrue(reloadedCartResult.Success,
                String.Join("|", reloadedCartResult.SystemMessages.Select(e => e.Message)));

            cart = reloadedCartResult.Cart;

            // Add billing address
            CommerceParty billingAddress = new CommerceParty();
            billingAddress.ExternalId =
                "Billing"; // This should correspond to the PartyId you are setting for the payment info
            billingAddress.PartyId = billingAddress.ExternalId;
            billingAddress.Name = "Billing";
            billingAddress.Address1 = "Dorpsstraat 50";
            billingAddress.Company = "Sitecore";
            billingAddress.Country = "The Netherlands";
            billingAddress.State = "ZH"; // State is checked: you can configure it in Commerce 
            billingAddress.CountryCode = "NL";
            billingAddress.LastName = "Werkman";
            billingAddress.FirstName = "Erwin";
            billingAddress.City = "Amsterdam";
            billingAddress.ZipPostalCode = "1234AK";

            cart.Parties.Add(billingAddress);

            // Add a payment address and payment method
            var payments = new List<PaymentInfo>();

            /*
             * For federated payment info to work, you need to set up the Braintree integration
            var federatedPaymentInfo = new FederatedPaymentInfo();

            federatedPaymentInfo.PartyID = billingAddress.ExternalId;
            federatedPaymentInfo.PaymentMethodID = "0CFFAB11-2674-4A18-AB04-228B1F8A1DEC";    // Federated Payment
            federatedPaymentInfo.Amount = cart.Total.Amount;   // Total payment (of all payment methods for a cart) should always be the same as the total amount of the order
            federatedPaymentInfo.CardToken = "payment-nonce";  // This should be valid b

            payments.Add(federatedPaymentInfo);
            */

            var giftCardPaymentInfo = new GiftCardPaymentInfo();
            giftCardPaymentInfo.PaymentMethodID = "B5E5464E-C851-4C3C-8086-A4A874DD2DB0"; // GiftCard
            giftCardPaymentInfo.Amount = cart.Total.Amount / 2;
            giftCardPaymentInfo.ExternalId = "GC1000000"; // This is the number of the giftcard
            giftCardPaymentInfo.PartyID = billingAddress.ExternalId;

            payments.Add(giftCardPaymentInfo);

            var simplePaymentInfo = new SimplePaymentInfo();
            simplePaymentInfo.PaymentMethodID = "7A6DF11A-CA92-4D7E-BF7C-1CDBF4F158E7";
            simplePaymentInfo.Amount = cart.Total.Amount / 2;

            payments.Add(simplePaymentInfo);

            var addPaymentInfoRequest = new AddPaymentInfoRequest(cart, payments);
            var addPaymentInfoResult = _cartServiceProvider.AddPaymentInfo(addPaymentInfoRequest);

            Assert.IsTrue(addPaymentInfoResult.Success,
                String.Join("|", addPaymentInfoResult.SystemMessages.Select(e => e.Message)));

            var freshCartRequest = new LoadCartByNameRequest(shopName, cartName, userId);
            var freshCartResult = _cartServiceProvider.LoadCart(reloadCartRequest);
            Assert.IsTrue(freshCartResult.Success,
                String.Join("|", freshCartResult.SystemMessages.Select(e => e.Message)));

            return View("Cart", freshCartResult);
        }

        public ActionResult BasketToOrder()
        {
            var shopName =
                "CommerceEngineDefaultStorefront"; // Better use a configured store, not CommerceEngineDefaultStorefront as it's not really configured
            var cartName = "Cart01";
            var userId = "1234";

            // Get a cart
            var loadCartRequest = new LoadCartByNameRequest(shopName, cartName, userId);
            var cartResult = _cartServiceProvider.LoadCart(loadCartRequest);

            Assert.IsTrue(cartResult.Success, String.Join("|", cartResult.SystemMessages.Select(e => e.Message)));

            var cart = cartResult.Cart;

            var quantity = 0.0M; // Get initial quantity
            if (cart.Lines.Count > 0)
            {
                quantity = cart.Lines[0].Quantity;
            }

            // Add a cart line: note that adding a line for the same product twice will update the quantity, not add a line: this is configured in commerce engine (look for RollupCartLinesPolicy)
            var lines = new List<CartLine>();
            var cartLine = new CommerceCartLine("Neu", "47838_aus_allen_sternen_liebe_cd", "", 1.0M);
            lines.Add(cartLine);

            var addLinesRequest = new AddCartLinesRequest(cart, lines);
            var addLinesResult = _cartServiceProvider.AddCartLines(addLinesRequest);

            Assert.IsTrue(addLinesResult.Success,
                String.Join("|", addLinesResult.SystemMessages.Select(e => e.Message)));

            var updatedCart = addLinesResult.Cart;

            Assert.IsTrue(updatedCart.Lines.Count > 0, "Updated cart should have at least one line");
            Assert.IsTrue(quantity + 1 == updatedCart.Lines[0].Quantity, "new quantity should be one more than before");

            var reloadCartRequest = new LoadCartByNameRequest(shopName, cartName, userId);
            var reloadedCartResult = _cartServiceProvider.LoadCart(reloadCartRequest);

            Assert.IsTrue(reloadedCartResult.Success,
                String.Join("|", reloadedCartResult.SystemMessages.Select(e => e.Message)));

            var reloadedCart = reloadedCartResult.Cart;

            Assert.IsTrue(reloadedCart.Lines.Count > 0, "Reloaded cart should have at least one line");
            Assert.IsTrue(quantity + 1 == reloadedCart.Lines[0].Quantity,
                "reloaded cart quantity should be one more than before");

            // Switching cart :-)
            cart = reloadedCart;

            // Add a shipping address
            CommerceParty shippingAddress = new CommerceParty();
            shippingAddress.ExternalId = "Shipping";
            shippingAddress.PartyId = shippingAddress.ExternalId;
            shippingAddress.Name = "Shipping";
            shippingAddress.Address1 = "Barbara Strozzilaan 201";
            shippingAddress.Company = "Sitecore";
            shippingAddress.Country = "Canada";
            shippingAddress.State = "ON"; // State is checked by commerce engine: you can configure it in Commerce 
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
            reloadedCartResult = _cartServiceProvider.LoadCart(reloadCartRequest);

            Assert.IsTrue(reloadedCartResult.Success,
                String.Join("|", reloadedCartResult.SystemMessages.Select(e => e.Message)));

            cart = reloadedCartResult.Cart;

            // Add billing address
            CommerceParty billingAddress = new CommerceParty();
            billingAddress.ExternalId =
                "Billing"; // This should correspond to the PartyId you are setting for the payment info
            billingAddress.PartyId = billingAddress.ExternalId;
            billingAddress.Name = "Billing";
            billingAddress.Address1 = "Dorpsstraat 50";
            billingAddress.Company = "Sitecore";
            billingAddress.Country = "The Netherlands";
            billingAddress.State = "ZH"; // State is checked: you can configure it in Commerce 
            billingAddress.CountryCode = "NL";
            billingAddress.LastName = "Werkman";
            billingAddress.FirstName = "Erwin";
            billingAddress.City = "Amsterdam";
            billingAddress.ZipPostalCode = "1234AK";

            cart.Parties.Add(billingAddress);

            // Add a payment address and payment method
            var payments = new List<PaymentInfo>();

            /*
             * For federated payment info to work, you need to set up the Braintree integration
            var federatedPaymentInfo = new FederatedPaymentInfo();

            federatedPaymentInfo.PartyID = billingAddress.ExternalId;
            federatedPaymentInfo.PaymentMethodID = "0CFFAB11-2674-4A18-AB04-228B1F8A1DEC";    // Federated Payment
            federatedPaymentInfo.Amount = cart.Total.Amount;   // Total payment (of all payment methods for a cart) should always be the same as the total amount of the order
            federatedPaymentInfo.CardToken = "payment-nonce";  // This should be valid b

            payments.Add(federatedPaymentInfo);
            */

            /*
            var giftCardPaymentInfo = new GiftCardPaymentInfo();
            giftCardPaymentInfo.PaymentMethodID = "B5E5464E-C851-4C3C-8086-A4A874DD2DB0"; // GiftCard
            giftCardPaymentInfo.Amount = cart.Total.Amount;
            giftCardPaymentInfo.ExternalId = "GC1000000"; // This is the number of the giftcard
            giftCardPaymentInfo.PartyID = billingAddress.ExternalId;
            
            payments.Add(giftCardPaymentInfo);
            */

            var simplePaymentInfo = new SimplePaymentInfo();
            simplePaymentInfo.PaymentMethodID = "7A6DF11A-CA92-4D7E-BF7C-1CDBF4F158E7";
            simplePaymentInfo.Amount = cart.Total.Amount;

            payments.Add(simplePaymentInfo);


            var addPaymentInfoRequest = new AddPaymentInfoRequest(cart, payments);
            var addPaymentInfoResult = _cartServiceProvider.AddPaymentInfo(addPaymentInfoRequest);

            Assert.IsTrue(addPaymentInfoResult.Success,
                String.Join("|", addPaymentInfoResult.SystemMessages.Select(e => e.Message)));

            var freshCartRequest = new LoadCartByNameRequest(shopName, cartName, userId);
            var freshCartResult = _cartServiceProvider.LoadCart(reloadCartRequest);
            Assert.IsTrue(freshCartResult.Success,
                String.Join("|", freshCartResult.SystemMessages.Select(e => e.Message)));

            cart.Email = "erwin.werkman@sitecore.com"; // This is necessary otherwise the cart will not become an order

            // Save the cart as an order
            var submitVisitorOrderRequest = new SubmitVisitorOrderRequest(cart);
            var submitVisitorOrderResult = _orderServiceProvider.SubmitVisitorOrder(submitVisitorOrderRequest);

            Assert.IsTrue(submitVisitorOrderResult.Success,
                String.Join("|", submitVisitorOrderResult.SystemMessages.Select(e => e.Message)));

            return View("Cart", freshCartResult);
        }

        public ActionResult AddPromotionCodeToCart()
        {
            var loadCartRequest = new LoadCartRequest("CommerceEngineDefaultStorefront", "Cart01", "1234");
            var loadCartResult = _cartServiceProvider.LoadCart(loadCartRequest);
            var cart = loadCartResult.Cart as CustomCart;

            string promotionCode = "H20OFF";

            var request = new AddPromoCodeRequest(cart, promotionCode);
            var serviceProvider = new CommerceCartServiceProvider();
            var result = serviceProvider.AddPromoCode(request);

            return View("Cart", result);
        }

        public ActionResult AddAdditionalPartyToCart()
        {
            Sitecore.Commerce.Core.Party party = new Sitecore.Commerce.Core.Party
            {
                AddressName = "Billing Address",
                Address1 = "Main Street",
                City = "Big City"
            };
            party.Policies = new ObservableCollection<Policy>();
            party.Policies.Add(new ExtendedPartyPolicy()
                {IsCompany = false, Gender = "M", Phone = "0123456789", Title = "Dr."});

            var container =
                EngineConnectUtility.GetShopsContainer(shopName: "CommerceEngineDefaultStorefront", customerId: "1234");
            var command = Proxy.DoCommand(container.AddParty("Cart01", party));

            return View();
        }
    }
}