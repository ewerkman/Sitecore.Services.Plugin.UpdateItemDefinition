namespace Sitecore.Services.Plugin.Sample.Pipelines.Blocks
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.ManagedLists;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;
    using System.Threading.Tasks;

    [PipelineDisplayName("Sitecore.Services.Plugin.Sample.Pipelines.Blocks.AddToNameListBlock")]
    public class AddToNameListBlock : PipelineBlock<CatalogContentArgument, CatalogContentArgument, CommercePipelineExecutionContext>
    {
        protected CommerceCommander Commander { get; set; }

        public AddToNameListBlock(CommerceCommander commander)
            : base(null)
        {
            this.Commander = commander;
        }

        public override Task<CatalogContentArgument> Run(CatalogContentArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");

            foreach(var sellableItem in arg.SellableItems)
            {
                var listMembershipsComponent = sellableItem.GetComponent<ListMembershipsComponent>();
                listMembershipsComponent.Memberships.Add($"ById-{sellableItem.Name}");
            }

            return Task.FromResult(arg);
        }
    }
}
