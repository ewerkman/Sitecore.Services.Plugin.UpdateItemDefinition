// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddPriceSnapshotCommand.cs" company="Sitecore Corporation">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;


    /// <summary>
    ///     This command will update the snapshot on an existing price card. It will only update if there is only one price snapshot.
    /// </summary>
    public class UpdatePriceSnapshotCommand : CommerceCommand
    {

        protected CommerceCommander Commander { get; set; }

        public UpdatePriceSnapshotCommand(CommerceCommander commander, IServiceProvider serviceProvider) : base(serviceProvider)
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
                    {   /// Find the snapshot (we are assuming there is only one snapshot we are updating. If there are more we don't know which one we should update.)
                        if(priceCard.Snapshots.Count == 1)
                        {
                            var snapshot = priceCard.Snapshots[0];
                            if(snapshot.Tiers.Any())
                            {
                                var tier = snapshot.Tiers[0];
                                tier.Price = price;
                            }
                            else
                            {
                                var snapshotStartDate = DateTimeOffset.Now;
                                int quantity = 1;
                                string currency = "EUR";

                                snapshot = new PriceSnapshotComponent();

                                snapshot.BeginDate = snapshotStartDate;
                                snapshot.Tiers.Add(new PriceTier(currency, quantity, price));                
                            }

                            var approvalComponent = snapshot.GetComponent<ApprovalComponent>();
                            approvalComponent.ModifyStatus(approvalStatusPolicy.Approved, "Automatically approved");

                            await Commander.PersistEntity(commerceContext, priceCard);
                        }
                    }
                    
                    // We assume the pricecard is already attached to the sellable item so we don't save it
                }

                return this;
            }
        }

        internal Task Process(CommerceContext currentContext, IList<Party> list)
        {
            throw new NotImplementedException();
        }
    }
}