using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commerce.Plugin.Sample.Payment.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Orders;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Commerce.Plugin.Sample.Payment.Pipelines.Blocks
{
    [PipelineDisplayName("Sitecore.Services.Plugin.Sample.Pipelines.Blocks.GetOrderPaymentDetailsViewBlock")]
    public class GetOrderPaymentDetailsViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly GetOnHoldOrderCartCommand _getOnHoldOrderCartCommand;
        
        public GetOrderPaymentDetailsViewBlock(GetOnHoldOrderCartCommand getOnHoldOrderCartCommand)
        {
            _getOnHoldOrderCartCommand = getOnHoldOrderCartCommand;
        }
        
        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: The argument cannot be null.");

            var request = context.CommerceContext.GetObject<EntityViewArgument>();
            if (string.IsNullOrEmpty(request?.ViewName)
                || (!request.ViewName.Equals(context.GetPolicy<KnownPaymentsViewsPolicy>().OrderPayments, StringComparison.OrdinalIgnoreCase)
                    && !request.ViewName.Equals(context.GetPolicy<KnownOrderViewsPolicy>().Master, StringComparison.OrdinalIgnoreCase)
                    && !request.ViewName.Equals(context.GetPolicy<KnownPaymentsViewsPolicy>().OrderPaymentDetails, StringComparison.OrdinalIgnoreCase))
                || !(request.Entity is Order)
                || !string.IsNullOrEmpty(request.ForAction))
            {
                return Task.FromResult(arg);
            }

            var order = (Order) request.Entity;
            if (!order.HasComponent<PaymentComponent>())
            {
                return Task.FromResult(arg);
            }

            List<PaymentComponent> payments;
            payments = order.EntityComponents.OfType<PaymentComponent>().ToList();
            
            if (request.ViewName.Equals(context.GetPolicy<KnownOrderViewsPolicy>().Master, StringComparison.OrdinalIgnoreCase)
                || request.ViewName.Equals(context.GetPolicy<KnownPaymentsViewsPolicy>().OrderPayments, StringComparison.OrdinalIgnoreCase))
            {
                var paymentsViews = request.ViewName.Equals(context.GetPolicy<KnownOrderViewsPolicy>().Master, StringComparison.OrdinalIgnoreCase)
                    ? arg.ChildViews.Cast<EntityView>().FirstOrDefault(ev => ev.Name.Equals(context.GetPolicy<KnownPaymentsViewsPolicy>().OrderPayments, StringComparison.OrdinalIgnoreCase))
                    : arg;
                paymentsViews?.ChildViews.Where(cv => cv.Name.Equals(context.GetPolicy<KnownPaymentsViewsPolicy>().OrderPaymentDetails, StringComparison.OrdinalIgnoreCase)).Cast<EntityView>().ToList().ForEach(
                    paymentView =>
                    {
                        var simplePayment = payments.FirstOrDefault(s => s.Id.Equals(paymentView.ItemId, StringComparison.OrdinalIgnoreCase)) as SimplePaymentComponent;
                        if (simplePayment == null)
                        {
                            return;
                        }

                        PopulatePaymentDetails(paymentView, simplePayment);
                    });

                return Task.FromResult(arg);
            }
            
            var simplePaymentDetail = payments.FirstOrDefault(s => s.Id.Equals(request.ItemId, StringComparison.OrdinalIgnoreCase)) as SimplePaymentComponent;
            if (simplePaymentDetail == null)
            {
                return Task.FromResult(arg);
            }

            PopulatePaymentDetails(arg, simplePaymentDetail);

            return Task.FromResult(arg);
        }
        
        protected virtual void PopulatePaymentDetails(EntityView view, SimplePaymentComponent simplePayment)
        {
            if (view == null || simplePayment == null)
            {
                return;
            }

            view.Properties.Add(new ViewProperty
            {
                Name = "ItemId",
                IsReadOnly = true,
                IsHidden = true,
                RawValue = simplePayment.Id
            });
            view.Properties.Add(new ViewProperty
            {
                Name = "Type",
                IsReadOnly = true,
                RawValue = simplePayment.GetType().Name
            });
            view.Properties.Add(new ViewProperty
            {
                Name = "Amount",
                IsReadOnly = true,
                RawValue = simplePayment.Amount.Amount
            });
            view.Properties.Add(new ViewProperty
            {
                Name = "Currency",
                IsReadOnly = true,
                RawValue = simplePayment.Amount.CurrencyCode
            });
        }
    }
}