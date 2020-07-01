namespace Commerce.Plugin.Sample.Payment.Pipelines.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Commerce.Plugin.Sample.Payment.Components;
    using Microsoft.Extensions.Logging;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.ManagedLists;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName(nameof(SettleSimplePaymentBlock))]
    public class SettleSimplePaymentBlock : PipelineBlock<SalesActivity, SalesActivity, CommercePipelineExecutionContext>
    {
        protected CommerceCommander Commander { get; set; }

        public SettleSimplePaymentBlock(CommerceCommander commander) : base(null)
        {
            this.Commander = commander;
        }

        public override Task<SalesActivity> Run(SalesActivity arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The order cannot be null.");

            var salesActivity = arg;
            var knownSalesActivityStatuses = context.GetPolicy<KnownSalesActivityStatusesPolicy>();
            if (!salesActivity.HasComponent<SimplePaymentComponent>()
                || !salesActivity.PaymentStatus.Equals(knownSalesActivityStatuses.Pending, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(salesActivity);
            }

            var payment = salesActivity.GetComponent<SimplePaymentComponent>();

            // Perform logic to check whether the payment was settled
            var settled = PaymentHasBeenSettled();

            // Settle sales activity
            if (settled)
            {
                context.Logger.LogInformation($"{this.Name} - Payment succeeded: {payment.Id}");
                salesActivity.PaymentStatus = knownSalesActivityStatuses.Settled;
            }

            return Task.FromResult(salesActivity);
        }

        private bool PaymentHasBeenSettled()
        {   // Randomly settle a payment
            var rnd = new Random();
            return rnd.Next(100) < 50;
        }
    }
}