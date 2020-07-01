using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;
using Sitecore.Services.Plugin.Sample.Entities;
using Sitecore.Services.Plugin.Sample.Pipelines.Arguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Services.Plugin.Sample.Pipelines
{
    public class CreateCounterPipeline : CommercePipeline<CreateCounterArgument, Counter>, ICreateCounterPipeline
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Commerce.Plugin.Ordernumber.Pipelines.CreateCounterPipeline" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public CreateCounterPipeline(IPipelineConfiguration<ICreateCounterPipeline> configuration, ILoggerFactory loggerFactory)
            : base(configuration, loggerFactory)
        {
        }
    }
}
