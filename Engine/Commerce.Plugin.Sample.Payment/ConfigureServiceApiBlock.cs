namespace Commerce.Plugin.Sample.Payment
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.OData.Builder;
    using Plugin.Sample.Payment.Components;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName(nameof(ConfigureServiceApiBlock))]
    public class ConfigureServiceApiBlock : PipelineBlock<ODataConventionModelBuilder, ODataConventionModelBuilder, CommercePipelineExecutionContext>
    {
        public override Task<ODataConventionModelBuilder> Run(ODataConventionModelBuilder modelBuilder, CommercePipelineExecutionContext context)
        {
            Condition.Requires(modelBuilder).IsNotNull($"{this.Name}: The argument cannot be null.");

            modelBuilder.AddEntityType(typeof(SimplePaymentComponent));

            ActionConfiguration addSimplePaymentConfiguration = modelBuilder.Action("AddSimplePayment");
            addSimplePaymentConfiguration.Parameter<string>("cartId");
            addSimplePaymentConfiguration.Parameter<SimplePaymentComponent>("payment");
            addSimplePaymentConfiguration.ReturnsFromEntitySet<CommerceCommand>("Commands");

            return Task.FromResult(modelBuilder);
        }
    }
}
