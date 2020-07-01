using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Services.Plugin.Sample.Pipelines.Blocks.DoActions
{
    [PipelineDisplayName("Sitecore.Services.Plugin.Sample.Pipelines.Blocks.DoActions.DoActionExtendAssociateSellableItemBlock")]
    public class DoActionExtendAssociateSellableItemBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CatalogCommander commander;
        private readonly IFindEntityPipeline findEntityPipeline;        

        public DoActionExtendAssociateSellableItemBlock(CatalogCommander commander, IFindEntityPipeline findEntityPipeline)
        {
            this.findEntityPipeline = findEntityPipeline;
            this.commander = commander;
        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: The argument cannot be null");

            var variantId = string.Empty;

            if (string.IsNullOrEmpty(arg.Action) ||
                !arg.Action.Equals(KnownCatalogActionsPolicy.AssociateSellableItemToCatalog,
                    StringComparison.OrdinalIgnoreCase) &&
                !arg.Action.Equals(KnownCatalogActionsPolicy.AssociateSellableItemToCategory,
                    StringComparison.OrdinalIgnoreCase) &&
                !arg.Action.Equals(KnownCatalogActionsPolicy.AssociateSellableItemToBundle,
                    StringComparison.OrdinalIgnoreCase))
            {
                return arg;
            }

            var entity = context.CommerceContext.GetObject<CommerceEntity>(p => p.Id.Equals(arg.EntityId, StringComparison.OrdinalIgnoreCase));
            if (entity == null)
            {
                return arg;
            }

            // Get the selected sellable item id from the view
            var viewsPolicy = context.GetPolicy<KnownCatalogViewsPolicy>();
            var sellableItemId = arg.Properties.FirstOrDefault(p => p.Name.Equals(viewsPolicy.SellableItem, StringComparison.OrdinalIgnoreCase))?.Value;

            if (sellableItemId != null && !sellableItemId.StartsWith(CommerceEntity.IdPrefix<SellableItem>(), StringComparison.OrdinalIgnoreCase))
            {
                context.Abort(
                    await context.CommerceContext.AddMessage(
                        context.GetPolicy<KnownResultCodes>().ValidationError,
                        "InvalidType",
                        new object[] { sellableItemId, CommerceEntity.IdPrefix<SellableItem>() },
                        "Type '{0}' is invalid. Expected: '{1}'").ConfigureAwait(false),
                    context);
                return null;
            }

            var targetEntity = await findEntityPipeline.RunWithResult(new FindEntityArgument(typeof(SellableItem), sellableItemId), context).ConfigureAwait(false);

            if(!(entity is SellableItem))
            {
                var catalogName = entity.FriendlyId.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries)[0];

                var sellableItem = targetEntity?.Value as SellableItem;
                if(sellableItem != null)
                {
                    var catalogsComponent = sellableItem.GetComponent<CatalogsComponent>();

                    // Find the catalogs component for the "Neu" catalog
                    var neuCatalogComponent = catalogsComponent.ChildComponents.OfType<CatalogComponent>().Where(c => c.Name.Equals("Neu", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (neuCatalogComponent != null)
                    {
                        // Find the catalogs component for the catalog this sellable item is being associated with
                        var catalogComponent = catalogsComponent.ChildComponents.OfType<CatalogComponent>().Where(c => c.Name.Equals(catalogName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (catalogComponent != null)
                        {   // We found the catalog component: if it already has an item definition, leave it as it is
                            if (string.IsNullOrEmpty(catalogComponent.ItemDefinition))
                            {
                                catalogComponent.ItemDefinition = neuCatalogComponent.ItemDefinition;

                                // Save the updated sellable item
                                await commander.PersistEntity(context.CommerceContext, sellableItem);
                            }
                        }
                    }
                }
            }

            return arg;
        }
    }
}
