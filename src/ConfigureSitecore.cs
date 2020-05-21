namespace Sitecore.Services.Plugin.Sample
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;
    using Sitecore.Services.Plugin.Sample.Pipelines.Blocks;

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
                    ));

            services.RegisterAllCommands(assembly);
        }
    }
}