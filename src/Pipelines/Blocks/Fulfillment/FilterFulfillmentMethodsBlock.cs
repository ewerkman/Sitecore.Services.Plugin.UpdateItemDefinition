// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterFulfillmentMethodsBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Services.Plugin.Sample.Pipelines.Blocks.Fulfillment
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Content;
    using Sitecore.Commerce.Plugin.Fulfillment;
    using Sitecore.Commerce.Plugin.Management;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;
    using Sitecore.Services.Core.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [PipelineDisplayName("Sitecore.Services.Plugin.Sample.Pipelines.Blocks.Fulfillment.FilterFulfillmentMethodsBlock")]
    public class FilterFulfillmentMethodsBlock : PipelineBlock<IEnumerable<FulfillmentMethod>, IEnumerable<FulfillmentMethod>, CommercePipelineExecutionContext>
    {
        protected ContentCommander Commander { get; set; }

        public FilterFulfillmentMethodsBlock(ContentCommander commander)
            : base(null)
        {
            this.Commander = commander;
        }

        public override async Task<IEnumerable<FulfillmentMethod>> Run(IEnumerable<FulfillmentMethod> arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");

            var cartPartyArgument = context.CommerceContext.GetObject<CartPartyArgument>();

            var currentShopName = context.CommerceContext.CurrentShopName();

            List<ItemModel> shippingMethodItems = await GetShippingMethodItems(context, currentShopName);

            List<string> validShippingMethodsForCountryCode = await GetValidShippingMethodsForCountryCode(context, cartPartyArgument.Party.CountryCode, shippingMethodItems);

            // Filter out the valid shipping methods from all shipping methods
            var methods = arg.Where(m => validShippingMethodsForCountryCode.Contains(m.Id));

            return methods.AsEnumerable();
        }

        private async Task<List<string>> GetValidShippingMethodsForCountryCode(CommercePipelineExecutionContext context, String countryCode, List<ItemModel> shippingMethodItems)
        {
            var validShippingMethods = new List<string>();

            foreach (var item in shippingMethodItems)
            {
                var shippingCountriesGuids = item["Shipping Countries"] as string;
                if (shippingCountriesGuids != null)
                {
                    var shippingCountries = shippingCountriesGuids.Split('|');
                    foreach (var shippingCountry in shippingCountries)
                    {
                        var shippingCountryItem = await Commander.GetContentItem(context.CommerceContext, shippingCountry.Replace('{', ' ').Replace('}', ' ').Trim());
                        if (shippingCountryItem != null)
                        {
                            var itemComponent = shippingCountryItem.GetComponent<ItemsComponent>();
                            foreach (var countryItem in itemComponent.Items)
                            {
                                var shippingMethodCountryCode = countryItem.Value["Country Code"] as string;
                                if (shippingMethodCountryCode == countryCode)
                                {
                                    validShippingMethods.Add(item["ItemID"] as string);
                                }
                            }
                        }
                    }
                }
            }

            return validShippingMethods;
        }

        /// <summary>
        ///  Retrieves Shipping method Sitecore items 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="currentShopName"></param>
        /// <returns></returns>
        private async Task<List<ItemModel>> GetShippingMethodItems(CommercePipelineExecutionContext context, string currentShopName)
        {
            var itemsPolicy = context.GetPolicy<SitecoreControlPanelItemsPolicy>();
            var fieldsPolicy = context.GetPolicy<SitecoreItemFieldsPolicy>();

            var path = context.CommerceContext.GetObject<KeyValuePair<string, string>>(kv => kv.Key.Equals("itemPath", StringComparison.OrdinalIgnoreCase)).Value ?? string.Empty;
            var itemPath = string.IsNullOrEmpty(path) ? $"{itemsPolicy?.StorefrontsPath}/{currentShopName}" : path;
            List<ItemModel> items = new List<ItemModel>();

            var storefrontConfigurationItem = await this.Commander.Pipeline<IGetItemByPathPipeline>().Run(new ItemModelArgument(itemPath), context).ConfigureAwait(false);
            if (storefrontConfigurationItem != null)
            {
                var storefrontConfigurationItems = await this.Commander.Pipeline<IGetItemsByPathPipeline>().Run(new ItemModelArgument(itemPath), context).ConfigureAwait(false);
                var fulfillmentConfigurationItem = await this.GetFulfillmentConfigurationItem(itemPath, storefrontConfigurationItems, itemsPolicy, context).ConfigureAwait(false);
                if (fulfillmentConfigurationItem != null)
                {
                    var optionsIds = fulfillmentConfigurationItem.ContainsKey(fieldsPolicy.FulfillmentOptions)
                           ? fulfillmentConfigurationItem[fieldsPolicy.FulfillmentOptions] as string
                           : string.Empty;
                    if (!string.IsNullOrEmpty(optionsIds))
                    {
                        var ids = optionsIds.Split('|');
                        if (ids.Any())
                        {
                            foreach (var id in ids)
                            {
                                var fulfillmentOptionItem = await this.Commander.Pipeline<IGetItemByIdPipeline>().Run(new ItemModelArgument(id), context).ConfigureAwait(false);
                                if (fulfillmentOptionItem == null)
                                {
                                    continue;
                                }

                                var methodsItems = await this.Commander.Pipeline<IGetItemsByPathPipeline>()
                                    .Run(new ItemModelArgument(fulfillmentOptionItem[ItemModel.ItemPath].ToString()), context).ConfigureAwait(false);
                                methodsItems?.ForEach(fo => items.Add(fo));
                            }
                        }
                    }
                }
            }

            return items;
        }

        public virtual async Task<ItemModel> GetFulfillmentConfigurationItem(string storefrontPath, IEnumerable<ItemModel> storefrontConfigurationItems, SitecoreControlPanelItemsPolicy policy, CommercePipelineExecutionContext context)
        {
            var configurationItemName = storefrontConfigurationItems.FirstOrDefault(i => i[ItemModel.TemplateName].ToString().Equals(policy.FulfillmentConfigurationTemplate, StringComparison.OrdinalIgnoreCase));
            if (configurationItemName == null)
            {
                return null;
            }

            var configurationPath = $"{storefrontPath}/{configurationItemName[ItemModel.ItemName]}";
            var configurationItem = await this.Commander.Pipeline<IGetItemByPathPipeline>().Run(new ItemModelArgument(configurationPath), context).ConfigureAwait(false);

            return configurationItem;
        }
    }
}