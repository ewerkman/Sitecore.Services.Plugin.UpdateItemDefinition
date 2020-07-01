// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RefundSimplePaymentBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2019
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Commerce.Plugin.Sample.Payment.Pipelines.Blocks
{
    using Commerce.Plugin.Sample.Payment.Components;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.ManagedLists;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Commerce.Plugin.Payments;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [PipelineDisplayName(nameof(RefundSimplePaymentBlock))]
    public class RefundSimplePaymentBlock : PipelineBlock<OrderPaymentsArgument, OrderPaymentsArgument, CommercePipelineExecutionContext>
    {

        protected CommerceCommander Commander { get; set; }

        public RefundSimplePaymentBlock(CommerceCommander commander)
            : base(null)
        {

            this.Commander = commander;

        }

        public async override Task<OrderPaymentsArgument> Run(OrderPaymentsArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name} The arg can not be null");
            Condition.Requires(arg.Order).IsNotNull($"{this.Name} The order can not be null");
            Condition.Requires(arg.Payments).IsNotNull($"{this.Name} The payments can not be null");

            var order = arg.Order;
            if (!order.HasComponent<SimplePaymentComponent>())
            {
                return arg;
            }
                        
            if (!order.Status.Equals(context.GetPolicy<KnownOrderStatusPolicy>().Completed, StringComparison.OrdinalIgnoreCase))
            {
                var invalidOrderStateMessage = $"{this.Name}: Expected order in '{context.GetPolicy<KnownOrderStatusPolicy>().Completed}' status but order was in '{order.Status}' status";
                await context.CommerceContext.AddMessage(
                        context.GetPolicy<KnownResultCodes>().ValidationError,
                        "InvalidOrderState",
                        new object[] { context.GetPolicy<KnownOrderStatusPolicy>().Completed, order.Status },
                        invalidOrderStateMessage);
                return null;
            }

            var existingPayment = order.GetComponent<SimplePaymentComponent>();
            var paymentToRefund = arg.Payments.FirstOrDefault(p => p.Id.Equals(existingPayment.Id, StringComparison.OrdinalIgnoreCase)) as SimplePaymentComponent;
            if (paymentToRefund == null)
            {
                return arg;
            }

            if (existingPayment.Amount.Amount < paymentToRefund.Amount.Amount)
            {
                await context.CommerceContext.AddMessage(
                    context.GetPolicy<KnownResultCodes>().Error,
                    "IllegalRefundOperation",
                    new object[] { order.Id, existingPayment.Id }, 
                    "Order Simple Payment amount is less than refund amount");
                return null;
            }

            // Perform logic to reverse the actual payment

            if (existingPayment.Amount.Amount == paymentToRefund.Amount.Amount)
            {
                // Remove the existingPayment from the order since the entire amount is refunded
                order.RemoveComponent(existingPayment.GetType());
            }
            else
            {
                // Reduce the existing existingPayment in the order
                existingPayment.Amount.Amount -= paymentToRefund.Amount.Amount;
            }

            await this.GenerateSalesActivity(order, existingPayment, paymentToRefund, context);

            return arg;
        }

        private async Task GenerateSalesActivity(Order order, 
            PaymentComponent existingPayment, 
            PaymentComponent paymentToRefund, 
            CommercePipelineExecutionContext context)
        {
            var salesActivity = new SalesActivity
            {
                Id = $"{CommerceEntity.IdPrefix<SalesActivity>()}{Guid.NewGuid():N}",
                ActivityAmount = new Money(existingPayment.Amount.CurrencyCode, paymentToRefund.Amount.Amount * -1),
                Customer = new EntityReference
                {
                    EntityTarget = order.EntityComponents.OfType<ContactComponent>().FirstOrDefault()?.CustomerId
                },
                Order = new EntityReference
                {
                    EntityTarget = order.Id
                },
                Name = "Simple Payment Refund",
                PaymentStatus = context.GetPolicy<KnownSalesActivityStatusesPolicy>().Completed
            };

            // Add the new sales activity to the OrderSalesActivities list
            salesActivity.SetComponent(new ListMembershipsComponent
            {
                Memberships = new List<string>
                    {
                        CommerceEntity.ListName<SalesActivity>(),
                        context.GetPolicy<KnownOrderListsPolicy>().SalesCredits,
                        string.Format(context.GetPolicy<KnownOrderListsPolicy>().OrderSalesActivities, order.FriendlyId)
                    }
            });

            if (existingPayment.Amount.Amount != paymentToRefund.Amount.Amount)
            {
                salesActivity.SetComponent(existingPayment);
            }

            var salesActivities = order.SalesActivity.ToList();
            salesActivities.Add(new EntityReference { EntityTarget = salesActivity.Id });
            order.SalesActivity = salesActivities;

            await Commander.PersistEntity(context.CommerceContext, salesActivity);
        }
    }
}