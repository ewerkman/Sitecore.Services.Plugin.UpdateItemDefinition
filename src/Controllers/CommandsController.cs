// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandsController.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2019
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Services.Plugin.Sample.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Services.Plugin.Sample.Commands;
    using Sitecore.Services.Plugin.Sample.Commands.Parties;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http.OData;

    public class CommandsController : CommerceController
    {
        public CommandsController(IServiceProvider serviceProvider, CommerceEnvironment globalEnvironment)
            : base(serviceProvider, globalEnvironment)
        {
        }

        [HttpPut]
        [Route("UpdateItemDefinition")]
        public async Task<IActionResult> UpdateItemDefinition([FromBody] ODataActionParameters value)
        {
            if (!this.ModelState.IsValid || value == null)
            {
                return (IActionResult)new BadRequestObjectResult(this.ModelState);
            }

            if (!value.ContainsKey("itemId") || !value.ContainsKey("itemDefinition"))
            {
                return (IActionResult)new BadRequestObjectResult((object)value);
            }

            var itemId = value["itemId"].ToString();
            var itemDefinition = value["itemDefinition"].ToString();

            var command = this.Command<UpdateItemDefinitionCommand>();
            await command.Process(this.CurrentContext, itemId, itemDefinition).ConfigureAwait(false);

            return new ObjectResult(command);
        }

        [HttpPut]
        [Route("CreatePriceCard")]
        public async Task<IActionResult> CreatePriceCard([FromBody] ODataActionParameters value)
        {
            if (!this.ModelState.IsValid || value == null)
            {
                return (IActionResult)new BadRequestObjectResult(this.ModelState);
            }

            if (!value.ContainsKey("itemId") || !value.ContainsKey("price"))
            {
                return (IActionResult)new BadRequestObjectResult((object)value);
            }

            var itemId = value["itemId"].ToString();
            var price = decimal.Parse( value["price"].ToString());

            var command = this.Command<CreatePriceCardCommand>();
            await command.Process(this.CurrentContext, itemId, price).ConfigureAwait(false);

            return new ObjectResult(command);
        }

        [HttpPut]
        [Route("AddNewPriceSnapshot")]
        public async Task<IActionResult> AddNewPriceSnapshot([FromBody] ODataActionParameters value)
        {
            if (!this.ModelState.IsValid || value == null)
            {
                return (IActionResult)new BadRequestObjectResult(this.ModelState);
            }

            if (!value.ContainsKey("itemId") || !value.ContainsKey("price"))
            {
                return (IActionResult)new BadRequestObjectResult((object)value);
            }

            var itemId = value["itemId"].ToString();
            var price = decimal.Parse(value["price"].ToString());

            var command = this.Command<AddNewPriceSnapshotCommand>();
            await command.Process(this.CurrentContext, itemId, price).ConfigureAwait(false);

            return new ObjectResult(command);
        }

        [HttpPut]
        [Route("UpdatePriceSnapshot")]
        public async Task<IActionResult> UpdatePriceSnapshot([FromBody] ODataActionParameters value)
        {
            if (!this.ModelState.IsValid || value == null)
            {
                return (IActionResult)new BadRequestObjectResult(this.ModelState);
            }

            if (!value.ContainsKey("itemId") || !value.ContainsKey("price"))
            {
                return (IActionResult)new BadRequestObjectResult((object)value);
            }

            var itemId = value["itemId"].ToString();
            var price = decimal.Parse(value["price"].ToString());

            var command = this.Command<UpdatePriceSnapshotCommand>();
            await command.Process(this.CurrentContext, itemId, price).ConfigureAwait(false);

            return new ObjectResult(command);
        }

        [HttpPut]
        [Route("AddParty")]
        public async Task<IActionResult> AddParty([FromBody] ODataActionParameters value)
        {
            if (!this.ModelState.IsValid || value == null)
            {
                return (IActionResult)new BadRequestObjectResult(this.ModelState);
            }

            if (!value.ContainsKey("cartId") || !value.ContainsKey("party"))
            {
                return (IActionResult)new BadRequestObjectResult((object)value);
            }

            var cartId = (string)value["cartId"];
            var party = ((JObject)value["party"]).ToObject<Party>();

            var command = this.Command<AddPartyCommand>();
            await command.Process(this.CurrentContext, cartId, party).ConfigureAwait(false);

            return new ObjectResult(command);
        }

        [HttpPut]
        [Route("RemoveParty")]
        public async Task<IActionResult> RemoveParty([FromBody] ODataActionParameters value)
        {
            if (!this.ModelState.IsValid || value == null)
            {
                return (IActionResult)new BadRequestObjectResult(this.ModelState);
            }

            if (!value.ContainsKey("cartId") || !value.ContainsKey("addressName"))
            {
                return (IActionResult)new BadRequestObjectResult((object)value);
            }

            var cartId = (string)value["cartId"];
            var addressName = (string)value["addressName"];

            var command = this.Command<RemovePartyCommand>();
            await command.Process(this.CurrentContext, cartId, addressName).ConfigureAwait(false);

            return new ObjectResult(command);
        }

        [HttpPut]
        [Route("CreateCounter")]
        public async Task<IActionResult> CreateCounter([FromBody] ODataActionParameters value)
        {
            if (!this.ModelState.IsValid)
            {
                return new BadRequestObjectResult(this.ModelState);
            }

            if (!value.ContainsKey("counterName"))
            {
                return new BadRequestObjectResult(value);
            }

            if (!value.ContainsKey("startValue"))
            {
                return new BadRequestObjectResult(value);
            }

            var counterName = (string)value["counterName"];
            var startValue = (long)value["startValue"];

            var command = Command<CreateCounterCommand>();

            var result = await command.Process(CurrentContext, counterName, startValue).ConfigureAwait(false);

            return new ObjectResult(command);
        }
    }
}