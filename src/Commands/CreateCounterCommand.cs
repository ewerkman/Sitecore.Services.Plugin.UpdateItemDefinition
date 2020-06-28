using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Services.Plugin.Sample.Entities;
using Sitecore.Services.Plugin.Sample.Pipelines;
using Sitecore.Services.Plugin.Sample.Pipelines.Arguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Services.Plugin.Sample.Commands
{
    public class CreateCounterCommand : CommerceCommand
    {
        protected CommerceCommander Commander { get; set; }

        public CreateCounterCommand(CommerceCommander commander, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.Commander = commander;
        }

        public async Task<Counter> Process(CommerceContext commerceContext, string counterName, long startValue = 0, long increment = 1)
        {
            Counter result = null;
            using (CommandActivity.Start(commerceContext, this))
            {
                await PerformTransaction(
                    commerceContext,
                    async () =>
                    {
                        var contextOptions = commerceContext.PipelineContextOptions;

                        var argument = new CreateCounterArgument(counterName, startValue, increment);

                        var counter = await Commander.Pipeline<ICreateCounterPipeline>().Run(argument, contextOptions).ConfigureAwait(false);
                        result = counter;

                    }).ConfigureAwait(false);

                return result;
            }
        }
    }
}
