using Sitecore.Commerce.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Services.Plugin.Sample.Pipelines.Arguments
{
    public class GetNextCounterValueArgument : PipelineArgument
    {
        public GetNextCounterValueArgument(string counterName)
        {
            CounterName = counterName;
        }

        public string CounterName { get; }
    }
}
