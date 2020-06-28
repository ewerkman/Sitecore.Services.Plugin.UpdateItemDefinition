using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;
using Sitecore.Services.Plugin.Sample.Entities;
using Sitecore.Services.Plugin.Sample.Pipelines.Arguments;

namespace Sitecore.Services.Plugin.Sample.Pipelines
{
    public interface ICreateCounterPipeline : IPipeline<CreateCounterArgument, Counter, CommercePipelineExecutionContext>
    {
    }
}
