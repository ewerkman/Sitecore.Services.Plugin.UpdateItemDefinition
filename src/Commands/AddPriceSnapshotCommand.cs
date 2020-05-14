// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdatePriceCardCommand.cs" company="Sitecore Corporation">
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

    public class AddNewPriceSnapshotCommand : CommerceCommand
    {
        protected CommerceCommander Commander { get; set; }

        public AddNewPriceSnapshotCommand(CommerceCommander commander, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.Commander = commander;
        }

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
                    var priceCardId = $"HerderPreisbuch-{productId}".ToEntityId<PriceCard>();
                    var priceCard = await Commander.GetEntity<PriceCard>(commerceContext, priceCardId);

                    if (priceCard != null)
                    {   /// Create a new snapshot... 
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

                        // Associate price card with sellable item (if this has not been done already)
                        var priceCardPolicy = sellableItem.GetPolicy<PriceCardPolicy>();
                        priceCardPolicy.PriceCardName = priceCard.Name;

                        await Commander.PersistEntity(commerceContext, sellableItem);
                    }

                    // We assume the pricecard is already attached to the sellable item so we don't save it
                }

                return this;
            }
        }
    }
}