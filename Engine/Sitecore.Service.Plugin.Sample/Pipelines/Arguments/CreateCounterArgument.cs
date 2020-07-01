using Sitecore.Commerce.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Services.Plugin.Sample.Pipelines.Arguments
{
    public class CreateCounterArgument : PipelineArgument
    {
        public CreateCounterArgument(string counterName, long startValue = 0, long increment = 1)
        {
            CounterName = counterName;
            StartValue = startValue;
            Increment = increment;
        }

        public string CounterName { get; }
        public long StartValue { get; }
        public long Increment { get; }
    }
}
