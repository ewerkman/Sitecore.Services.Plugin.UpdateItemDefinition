// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiController.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Services.Plugin.Sample.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using Sitecore.Commerce.Core;
    using Sitecore.Services.Plugin.Sample.Commands;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http.OData;

    /// <inheritdoc />
    /// <summary>
    /// Defines an api controller
    /// </summary>
    /// <seealso cref="T:Sitecore.Commerce.Core.CommerceController" />
    [Route("api")]
    public class ApiController : CommerceController
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Services.Plugin.Sample.Controllers.ApiController" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="globalEnvironment">The global environment.</param>
        public ApiController(IServiceProvider serviceProvider, CommerceEnvironment globalEnvironment)
            : base(serviceProvider, globalEnvironment)
        {
        }

        [HttpPost]
        [Route("GetDeliveryTime")]
        public async Task<IActionResult> GetDeliveryTime([FromBody] ODataActionParameters value)
        {
            if (!this.ModelState.IsValid)
            {
                return new BadRequestObjectResult(this.ModelState);
            }

            if (!value.ContainsKey("itemIds") || (value["itemIds"] as JArray) == null ||
                !value.ContainsKey("primaryInventorySetId") || (value["primaryInventorySetId"] as string) == null ||
                !value.ContainsKey("secondaryInventorySetId") || (value["secondaryInventorySetId"] as string) == null)
            {
                return new BadRequestObjectResult(value);
            }

            var itemIds = (JArray)value["itemIds"];
            var primaryInventorySetId = value["primaryInventorySetId"] as string;
            var secondaryInventorySetId = value["secondaryInventorySetId"] as string;

            var result = await Command<GetDeliveryTimeCommand>().Process(CurrentContext, itemIds?.ToObject<IEnumerable<string>>(), primaryInventorySetId, secondaryInventorySetId).ConfigureAwait(false);

            return new ObjectResult(result);
        }
    }
}