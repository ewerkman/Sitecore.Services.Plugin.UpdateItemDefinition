// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeliveryTimeModel.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Services.Plugin.Sample.Models
{
    using Sitecore.Commerce.Core;

    /// <inheritdoc />
    /// <summary>
    /// The DeliveryTimeModel.
    /// </summary>
    public class DeliveryTime : Model
    {
        public DeliveryTime(string itemId)
        {
            this.ItemId = itemId;
        }

        public string ItemId { get; set; }
        public int DeliveryTimeInDays { get; set; }
        public int AvailableQuantity { get; set; } = 0;
    }
}