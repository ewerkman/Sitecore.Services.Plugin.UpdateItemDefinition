// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddArgumentToContextBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Services.Plugin.Sample.Pipelines.Blocks.Fulfillment
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Fulfillment;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;
    using System.Threading.Tasks;

    [PipelineDisplayName("Sitecore.Services.Plugin.Sample.Pipelines.Blocks.Fulfillment.AddArgumentToContextBlock")]
    public class AddArgumentToContextBlock : PipelineBlock<CartLinePartyArgument, CartLinePartyArgument, CommercePipelineExecutionContext>
    {
        protected CommerceCommander Commander { get; set; }

        public AddArgumentToContextBlock(CommerceCommander commander)
            : base(null)
        {
            this.Commander = commander;
        }

        public override Task<CartLinePartyArgument> Run(CartLinePartyArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");

            context.CommerceContext.AddObject(arg);

            return Task.FromResult(arg);
        }
    }
}