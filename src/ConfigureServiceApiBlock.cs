// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureServiceApiBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Services.Plugin.Sample
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.OData.Builder;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;
    using Sitecore.Services.Plugin.Sample.Models;

    /// <summary>
    /// Defines a block which configures the OData model
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{Microsoft.AspNetCore.OData.Builder.ODataConventionModelBuilder,
    ///         Microsoft.AspNetCore.OData.Builder.ODataConventionModelBuilder,
    ///         Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    [PipelineDisplayName("Sitecore.Services.Plugin.Sample.ConfigureServiceApiBlock")]
    public class ConfigureServiceApiBlock : PipelineBlock<ODataConventionModelBuilder, ODataConventionModelBuilder, CommercePipelineExecutionContext>
    {
        public override Task<ODataConventionModelBuilder> Run(ODataConventionModelBuilder modelBuilder, CommercePipelineExecutionContext context)
        {
            Condition.Requires(modelBuilder).IsNotNull($"{this.Name}: The argument cannot be null.");

            ActionConfiguration updateItemDefinitionAction = modelBuilder.Action("UpdateItemDefinition");
            updateItemDefinitionAction.Parameter<string>("itemId");
            updateItemDefinitionAction.Parameter<string>("itemDefinition");
            updateItemDefinitionAction.ReturnsFromEntitySet<CommerceCommand>("Commands");

            ActionConfiguration createPriceCardAction = modelBuilder.Action("CreatePriceCard");
            createPriceCardAction.Parameter<string>("itemId");
            createPriceCardAction.Parameter<decimal>("price");
            createPriceCardAction.ReturnsFromEntitySet<CommerceCommand>("Commands");

            ActionConfiguration updatePriceSnapshotAction = modelBuilder.Action("UpdatePriceSnapshot");
            updatePriceSnapshotAction.Parameter<string>("itemId");
            updatePriceSnapshotAction.Parameter<decimal>("price");
            updatePriceSnapshotAction.ReturnsFromEntitySet<CommerceCommand>("Commands");

            ActionConfiguration addNewPriceSnapshotAction = modelBuilder.Action("AddNewPriceSnapshot");
            addNewPriceSnapshotAction.Parameter<string>("itemId");
            addNewPriceSnapshotAction.Parameter<decimal>("price");
            addNewPriceSnapshotAction.ReturnsFromEntitySet<CommerceCommand>("Commands");

            ActionConfiguration getDeliveryTimeAction = modelBuilder.Action("GetDeliveryTime");
            addNewPriceSnapshotAction.CollectionParameter<string>("itemIds");
            addNewPriceSnapshotAction.Parameter<string>("primaryInventorySetId");
            addNewPriceSnapshotAction.Parameter<string>("secondaryInventorySetId");
            addNewPriceSnapshotAction.ReturnsCollection<DeliveryTime>();

            return Task.FromResult(modelBuilder);
        }
    }
}
