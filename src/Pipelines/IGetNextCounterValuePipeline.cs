using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;
using Sitecore.Services.Plugin.Sample.Pipelines.Arguments;

namespace Sitecore.Services.Plugin.Sample.Pipelines
{
    public interface IGetNextCounterValuePipeline : IPipeline<GetNextCounterValueArgument, long, CommercePipelineExecutionContext>
    {
    }
}
