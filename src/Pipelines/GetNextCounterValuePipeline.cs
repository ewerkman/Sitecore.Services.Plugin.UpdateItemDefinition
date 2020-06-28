using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;
using Sitecore.Services.Plugin.Sample.Pipelines.Arguments;

namespace Sitecore.Services.Plugin.Sample.Pipelines.Blocks
{
    public class GetNextCounterValuePipeline : CommercePipeline<GetNextCounterValueArgument, long>, IGetNextCounterValuePipeline
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Commerce.Plugin.Ordernumber.Pipelines.GetNextCounterValuePipeline" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public GetNextCounterValuePipeline(IPipelineConfiguration<IGetNextCounterValuePipeline> configuration, ILoggerFactory loggerFactory)
            : base(configuration, loggerFactory)
        {
        }
    }
}
