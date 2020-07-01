using Commerce.Plugin.Sample.Payment.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Engine.Connect;
using Sitecore.Commerce.Engine.Connect.Pipelines;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.ServiceProxy;
using Sitecore.Commerce.Services.Carts;
using System;
using System.Globalization;
using System.Linq;
using Sample.Commerce.Engine.Connect.Entities;

namespace Sample.Commerce.Engine.Connect.Pipelines.Carts
{
    public class AddSimplePaymentToCart
    {
        public void Process(ServicePipelineArgs args)
        {
            AddPaymentInfoRequest request;
            AddPaymentInfoResult result;

            request = args.Request as AddPaymentInfoRequest;
            result = args.Result as AddPaymentInfoResult;

            if( request.Payments.Any(p => p is SimplePaymentInfo))
            {
                var cart = request.Cart;
                var container = EngineConnectUtility.GetShopsContainer(shopName: cart.ShopName, customerId: cart.CustomerId);

                foreach (var payment in request.Payments.Where(p => p is SimplePaymentInfo).Select(p => TranslatePayment((SimplePaymentInfo) p)).ToList())
                {
                    var command = Proxy.DoCommand(container.AddSimplePayment(cart.ExternalId, payment));
                    result.HandleCommandMessages(command);                   
                }

                // Remove the SimplePaymentInfo payments from the list of payments, so they are not evaluated by other processors
                request.Payments = request.Payments.Where(p => !(p is SimplePaymentInfo)).ToList();
            }

        }

        private SimplePaymentComponent TranslatePayment(SimplePaymentInfo paymentInfo)
        {
            return new SimplePaymentComponent()
            {
                Id = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                PaymentMethod = new EntityReference { EntityTarget = paymentInfo.PaymentMethodID },
                Amount = Money.CreateMoney(paymentInfo.Amount)
            };
        }
    }
}