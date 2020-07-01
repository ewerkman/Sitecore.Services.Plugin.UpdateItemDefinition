namespace Plugin.Sample.Payment.Controllers
{
    using Commerce.Plugin.Sample.Payment.Components;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Payments;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http.OData;

    public class CommandsController : CommerceController
    {
        public CommandsController(IServiceProvider serviceProvider, 
            CommerceEnvironment globalEnvironment)
            : base(serviceProvider, globalEnvironment)
        {
        }

        [HttpPut]
        [Route("AddSimplePayment()")]
        public async Task<IActionResult> AddSimplePayment([FromBody] ODataActionParameters value)
        {
            if (!this.ModelState.IsValid || value == null)
            {
                return (IActionResult)new BadRequestObjectResult(this.ModelState);
            }

            if (!value.ContainsKey("cartId") || 
                (string.IsNullOrEmpty(value["cartId"]?.ToString()) || 
                !value.ContainsKey("payment")) || 
                string.IsNullOrEmpty(value["payment"]?.ToString()))
            {
                return (IActionResult)new BadRequestObjectResult((object)value);
            }

            string cartId = value["cartId"].ToString();

            var paymentComponent = JsonConvert.DeserializeObject<SimplePaymentComponent>(value["payment"].ToString());
            var command = this.Command<AddPaymentsCommand>();
            await command.Process(this.CurrentContext, cartId, new List<PaymentComponent> { paymentComponent });

            return new ObjectResult(command);
        }
    }
}