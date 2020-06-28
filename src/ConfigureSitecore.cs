namespace Sitecore.Services.Plugin.Sample
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;
    using Sitecore.Commerce.Plugin.Fulfillment;
    using Sitecore.Services.Plugin.Sample.Pipelines.Blocks;
    using Sitecore.Services.Plugin.Sample.Pipelines.Blocks.DoActions;
    using Sitecore.Services.Plugin.Sample.Pipelines.Blocks.Fulfillment;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Services.Plugin.Sample.Pipelines;

    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            // Configure pipelines
            services.Sitecore().Pipelines(config => config
              .ConfigurePipeline<IConfigureServiceApiPipeline>(configure => configure.Add<ConfigureServiceApiBlock>())
              );

            services.Sitecore().Pipelines(config =>
                    config.ConfigurePipeline<ICreateSellableItemPipeline>(c =>
                       c.Add<AddToNameListBlock>().After<CreateSellableItemBlock>()
                    )
                    .ConfigurePipeline<IDoActionPipeline>(c =>
                        c.Add<DoActionExtendAssociateSellableItemBlock>().After<DoActionAssociateSellableItemBlock>()
                    )
                    .ConfigurePipeline<IGetCartLineFulfillmentMethodsPipeline>(c =>
                        c.Add<AddArgumentToContextBlock>().Before<FilterCartLineFulfillmentMethodsBlock>()
                         .Add<FilterFulfillmentMethodsBlock>().After<FilterCartLineFulfillmentMethodsBlock>()
                     )
                     .ConfigurePipeline<IOrderPlacedPipeline>(c =>
                        c.Replace<OrderPlacedAssignConfirmationIdBlock, OrderPlacedAssignOrderNumberBlock>()
                        )
                    .AddPipeline<ICreateCounterPipeline, CreateCounterPipeline>(pipeline => pipeline
                       .Add<CreateCounterBlock>()
                       .Add<PersistCounterBlock>()
                       )
                  .AddPipeline<IGetNextCounterValuePipeline, GetNextCounterValuePipeline>(pipeline => pipeline
                       .Add<GetNextCounterValueBlock>()
                        )
                );

            services.RegisterAllCommands(assembly);
        }
    }
}