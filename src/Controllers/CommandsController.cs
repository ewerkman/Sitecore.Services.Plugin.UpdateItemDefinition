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
    using System;
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
    }
}