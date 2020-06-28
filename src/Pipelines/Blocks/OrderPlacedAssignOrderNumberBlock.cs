using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Services.Plugin.Sample.Commands;
using Sitecore.Services.Plugin.Sample.Components;
using System.Threading.Tasks;

namespace Sitecore.Services.Plugin.Sample.Pipelines.Blocks
{
    public class OrderPlacedAssignOrderNumberBlock : PipelineBlock<Sitecore.Commerce.Plugin.Orders.Order, Sitecore.Commerce.Plugin.Orders.Order, CommercePipelineExecutionContext>
    {
        protected CommerceCommander Commander { get; set; }

        public OrderPlacedAssignOrderNumberBlock(CommerceCommander commander) : base(null)
        {
            this.Commander = commander;
        }

        /// <summary>
        ///     Assigns the final order number to the order based on the history of assigned order number. 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<Commerce.Plugin.Orders.Order> Run(Commerce.Plugin.Orders.Order arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull("The order can not be null");

            await Commander.Command<AssignOrdernumberCommand>().Process(context.CommerceContext, arg);
            
            var ordernumberHistoryComponent = arg.GetComponent<OrdernumberHistoryComponent>();
            var lastOrdernumberHistoryEntry = ordernumberHistoryComponent.GetLastEntry();

            arg.OrderConfirmationId = lastOrdernumberHistoryEntry.OrderNumber;
            
            return arg;
        }
    }
}
