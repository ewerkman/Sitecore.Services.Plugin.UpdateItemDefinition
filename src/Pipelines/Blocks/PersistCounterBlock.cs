using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Services.Plugin.Sample.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Services.Plugin.Sample.Pipelines.Blocks
{
    public class PersistCounterBlock : PipelineBlock<Counter, Counter, CommercePipelineExecutionContext>
    {

        protected CommerceCommander Commander { get; set; }

        public PersistCounterBlock(CommerceCommander commander)
            : base(null)
        {

            this.Commander = commander;

        }

        public override async Task<Counter> Run(Counter counter, CommercePipelineExecutionContext context)
        {
            Condition.Requires(counter).IsNotNull($"{this.Name}: The argument can not be null");

            await Commander.PersistEntity(context.CommerceContext, counter);

            return counter;
        }
    }
}
