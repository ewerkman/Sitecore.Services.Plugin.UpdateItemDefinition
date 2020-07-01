// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreatePriceCardCommand.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Services.Plugin.Sample.Commands
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Pricing;
    using System;
    using System.Threading.Tasks;

    /// <inheritdoc />
    /// <summary>
    /// Defines the CreatePriceCardCommand command.
    /// </summary>
    public class CreatePriceCardCommand : CommerceCommand
    {
        /// <summary>
        /// Gets or sets the commander.
        /// </summary>
        /// <value>
        /// The commander.
        /// </value>
        protected CommerceCommander Commander { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Services.Plugin.Sample.Commands.CreatePriceCardCommand" /> class.
        /// </summary>
        /// <param name="pipeline">
        /// The pipeline.
        /// </param>
        /// <param name="serviceProvider">The service provider</param>
        public CreatePriceCardCommand(CommerceCommander commander, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.Commander = commander;
        }

        /// <summary>
        /// The process of the command
        /// </summary>
        /// <param name="commerceContext">
        /// The commerce context
        /// </param>
        /// <param name="parameter">
        /// The parameter for the command
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<CommerceCommand> Process(CommerceContext commerceContext, string itemId, decimal price)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var ids = itemId.Split('|');
                var catalogName = ids[0];
                var productId = ids[1];

                var approvalStatusPolicy = commerceContext.GetPolicy<ApprovalStatusPolicy>();

                var sellableItem = await Commander.GetEntity<SellableItem>(commerceContext, productId.ToEntityId<SellableItem>());

                if (sellableItem != null)
                {   
                    var priceCard = await Commander.Command<AddPriceCardCommand>().Process(commerceContext, "HerderPreisbuch", "product1");

                    if (priceCard != null)
                    {   /// Create a snapshot... 
                        var snapshotStartDate = DateTimeOffset.Now;
                        int quantity = 1;
                        string currency = "EUR";

                        var priceSnapShot = new PriceSnapshotComponent();

                        priceSnapShot.BeginDate = snapshotStartDate;
                        priceSnapShot.Tiers.Add(new PriceTier(currency, quantity, price));

                        // Add the snapshot to the price card
                        priceCard = await Commander.Command<Sitecore.Commerce.Plugin.Pricing.AddPriceSnapshotCommand>().Process(commerceContext, priceCard, priceSnapShot);

                        // Approve the snapshot
                        var approvalComponent = priceSnapShot.GetComponent<ApprovalComponent>();
                        approvalComponent.ModifyStatus(approvalStatusPolicy.Approved, "Automatically approved");

                        // Save the price card
                        await Commander.PersistEntity(commerceContext, priceCard);

                        // Associate price card with sellable item
                        var priceCardPolicy = sellableItem.GetPolicy<PriceCardPolicy>();
                        priceCardPolicy.PriceCardName = priceCard.Name;

                        await Commander.PersistEntity(commerceContext, sellableItem);
                    }
                }

                return this;
            }
        }
    }
}