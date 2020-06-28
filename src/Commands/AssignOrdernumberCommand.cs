namespace Sitecore.Services.Plugin.Sample.Commands
{
    using BSitecore.Services.Plugin.Sample.Commands;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Services.Plugin.Sample.Components;
    using Sitecore.Services.Plugin.Sample.Pipelines.Blocks;
    using System;
    using System.Threading.Tasks;

    public class AssignOrdernumberCommand : CommerceCommand
    {
        protected CommerceCommander Commander { get; set; }

        public AssignOrdernumberCommand(CommerceCommander commander, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.Commander = commander;
        }

        public async Task<string> Process(CommerceContext commerceContext, string cartId)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var cart = await Commander.GetEntity<Cart>(commerceContext, cartId);
                return await Process(commerceContext, cart);
            }
        }


        public async Task<string> Process(CommerceContext commerceContext, Cart cart)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                if (cart != null)
                {
                    var ordernumberHistoryComponent = cart.GetComponent<OrdernumberHistoryComponent>();

                    var nextOrderCounter = await Commander.Command<GetNextValueCommand>().Process(commerceContext, "CustomOrdernumber");

                    var nextOrderNumber = nextOrderCounter.ToString("D");
                    var ordernumberHistoryEntry = ordernumberHistoryComponent.Add(nextOrderNumber);

                    await Commander.PersistEntity(commerceContext, cart);

                    return ordernumberHistoryEntry.OrderNumber;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public async Task<Sitecore.Commerce.Plugin.Orders.Order> Process(CommerceContext commerceContext, Sitecore.Commerce.Plugin.Orders.Order order)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var ordernumberHistoryComponent = order.GetComponent<OrdernumberHistoryComponent>();
                if (ordernumberHistoryComponent.GetLastEntry() == null)
                {   // There are no entries yet, so we assign an order number now
                    var nextOrderCounter = await Commander.Command<GetNextValueCommand>().Process(commerceContext, "CustomOrdernumber");

                    var nextOrderNumber = nextOrderCounter.ToString("D");
                    var ordernumberHistoryEntry = ordernumberHistoryComponent.Add(nextOrderNumber);
                }

                return order;
            }
        }
    }
}