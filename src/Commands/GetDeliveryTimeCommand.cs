// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetDeliveryTimeCommand.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Services.Plugin.Sample.Commands
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Commerce.Plugin.Inventory;
    using Sitecore.Services.Plugin.Sample.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class GetDeliveryTimeCommand : CommerceCommand
    {
        protected InventoryCommander Commander { get; set; }

        public GetDeliveryTimeCommand(InventoryCommander commander,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.Commander = commander;
        }

        public async Task<List<DeliveryTime>> Process(CommerceContext commerceContext, IEnumerable<string> itemIds, string primaryInventorySetId, string secondaryInventorySetId)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var result = new List<DeliveryTime>();

                foreach(var itemId in itemIds)
                {
                    var inventoryInformations = new Dictionary<string, InventoryInformation>();

                    var sellableItemInventorySetArgument = new SellableItemInventorySetArgument(itemId, primaryInventorySetId);

                    var primaryInventory = await Commander.GetInventoryInformation(commerceContext, sellableItemInventorySetArgument);

                    sellableItemInventorySetArgument.InventorySetId = secondaryInventorySetId;
                    var secondaryInventory = await Commander.GetInventoryInformation(commerceContext, sellableItemInventorySetArgument);

                    var deliveryTime = new DeliveryTime(itemId);

                    result.Add(deliveryTime);

                    deliveryTime.DeliveryTimeInDays = -1;       // Don't know the delivery time
                    
                    if (primaryInventory != null && primaryInventory.Quantity > 0)
                    {
                        deliveryTime.DeliveryTimeInDays = 1;
                        deliveryTime.AvailableQuantity = primaryInventory.Quantity;
                    }
                    else if(secondaryInventory != null && secondaryInventory.Quantity > 0)
                    {
                        deliveryTime.DeliveryTimeInDays = 4;
                        deliveryTime.AvailableQuantity = secondaryInventory.Quantity;
                    }
                }

                return result ?? new List<DeliveryTime>();
            }
        }
    }
}