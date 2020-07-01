// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssignItemDefinitionCommand.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2019
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Services.Plugin.Sample.Commands
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Commerce.Plugin.Catalog;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class UpdateItemDefinitionCommand : CommerceCommand
    {
        protected CommerceCommander Commander { get; set; }

        public UpdateItemDefinitionCommand(CommerceCommander commander, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.Commander = commander;
        }

        public async Task<SellableItem> Process(CommerceContext commerceContext, string itemId, string itemDefinition)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var contextOptions = commerceContext.PipelineContextOptions;

                if (string.IsNullOrEmpty(itemId) || itemId.Split('|').Length != 3)
                {
                    await contextOptions.CommerceContext.AddMessage(
                       commerceContext.GetPolicy<KnownResultCodes>().Error,
                       "ItemIdIncorrectFormat",
                       new object[] { itemId },
                       $"Expecting a CatalogId and a ProductId in the ItemId: { itemId }. Correct format is 'catalogId|productId|[variantId]'."
                    ).ConfigureAwait(false);
                    return (SellableItem)null;
                }

                var ids = itemId.Split('|');
                var catalogName = ids[0];
                var productId = ids[1];

                var sellableItemEntityId = productId.ToEntityId<SellableItem>();

                // Get the sellable item we want to update
                var sellableItem = await Commander.GetEntity<SellableItem>(commerceContext, sellableItemEntityId);
                
                if(sellableItem != null)
                {
                    // The item definition is stored in a CatalogsComponent
                    var catalogsComponent = sellableItem.HasComponent<CatalogsComponent>() ? sellableItem.GetComponent<CatalogsComponent>() : null;
                    if(catalogsComponent != null)
                    {
                        // The CatalogsComponent has a CatalogComponent for each catalog the sellable item is attached to: we look up the catalog we want to change
                        var catalogComponent = catalogsComponent.Catalogs.FirstOrDefault(c => c.Name.Equals(catalogName, StringComparison.InvariantCultureIgnoreCase));
                        if(catalogComponent != null)
                        {   // Update the item definition and save the updated SellableItem
                            catalogComponent.ItemDefinition = itemDefinition;

                            var result = await Commander.PersistEntity(commerceContext, sellableItem);
                        }
                    }
                }

                return sellableItem;
            }
        }
    }
}