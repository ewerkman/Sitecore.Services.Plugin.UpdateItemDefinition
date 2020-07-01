namespace Sitecore.Services.Plugin.Sample.Components
{
    using Sitecore.Commerce.Core;
    using System;
    using System.Collections.Generic;

    /// <inheritdoc />
    /// <summary>
    /// The OrdernumberHistoryComponent.
    /// </summary>
    public class OrdernumberHistoryComponent : Component
    {
        public OrdernumberHistoryComponent()
        {
            OrdernumberHistory = new List<OrdernumberHistoryEntry>();
        }

        public List<OrdernumberHistoryEntry> OrdernumberHistory { get; }

        public string LatestOrderNumber { get; set; }  // Contains the latest assigned order number (for easy reference)

        public OrdernumberHistoryEntry Add(string orderNumber)
        {
            var ordernumberHistoryEntry = new OrdernumberHistoryEntry(orderNumber, DateTimeOffset.Now);

            OrdernumberHistory.Add(ordernumberHistoryEntry);

            LatestOrderNumber = ordernumberHistoryEntry.OrderNumber;

            return ordernumberHistoryEntry;
        }

        /// <summary>
        ///     Returns the ordernumberhistoryentry that was last added to the list
        /// </summary>
        /// <returns></returns>
        public OrdernumberHistoryEntry GetLastEntry()
        {
            if (OrdernumberHistory.Count > 0)
            {
                return OrdernumberHistory[OrdernumberHistory.Count - 1];
            }

            return null;
        }
    }

    public class OrdernumberHistoryEntry
    {
        public OrdernumberHistoryEntry(string orderNumber, DateTimeOffset assignedAt)
        {
            this.OrderNumber = orderNumber;
            this.AssignedAt = assignedAt;
        }

        public string OrderNumber { get; }
        public DateTimeOffset AssignedAt { get; }
    }
}

