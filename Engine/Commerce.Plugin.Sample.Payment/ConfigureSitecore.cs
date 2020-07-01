using Sitecore.Commerce.EntityViews;

namespace Commerce.Plugin.Sample.Payment
{
    using System.Reflection;
    using Commerce.Plugin.Sample.Payment.Pipelines.Blocks;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Commerce.Plugin.Payments;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config
                .ConfigurePipeline<IConfigureServiceApiPipeline>(c => c.Add<ConfigureServiceApiBlock>())
                .ConfigurePipeline<IRefundPaymentsPipeline>(c => c.Add<RefundSimplePaymentBlock>().Before<PersistOrderBlock>())
                .ConfigurePipeline<IGetEntityViewPipeline>(c =>
                    c.Add<Pipelines.Blocks.GetOrderPaymentDetailsViewBlock>().After<Sitecore.Commerce.Plugin.Payments.GetOrderPaymentDetailsViewBlock>())
            );

            services.RegisterAllCommands(assembly);
        }
    }
}