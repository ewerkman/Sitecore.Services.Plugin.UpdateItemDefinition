using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Services.Plugin.Sample.Pipelines;
using Sitecore.Services.Plugin.Sample.Pipelines.Arguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSitecore.Services.Plugin.Sample.Commands
{
    public class GetNextValueCommand : CommerceCommand
    {
        /// <summary>
        /// Gets or sets the commander.
        /// </summary>
        /// <value>
        /// The commander.
        /// </value>
        protected CommerceCommander Commander { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Commerce.Plugin.Ordernumber.Commands.GetNextValueCommand" /> class.
        /// </summary>
        /// <param name="pipeline">
        /// The pipeline.
        /// </param>
        /// <param name="serviceProvider">The service provider</param>
        public GetNextValueCommand(CommerceCommander commander, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.Commander = commander;
        }

        /// <summary>
        /// The process of the command
        /// </summary>
        /// <param name="commerceContext">
        /// The commerce context
        /// </param>
        /// <param name="parameter">
        /// The parameter for the command
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<long> Process(CommerceContext commerceContext, string counterName)
        {
            using (CommandActivity.Start(commerceContext, this))
            {
                long nextValue = -1;
                await PerformTransaction(
                    commerceContext,
                    async () =>
                    {
                        var contextOptions = commerceContext.PipelineContextOptions;

                        var argument = new GetNextCounterValueArgument(counterName);

                        nextValue = await Commander.Pipeline<IGetNextCounterValuePipeline>().Run(argument, contextOptions).ConfigureAwait(false);

                    }).ConfigureAwait(false);

                return nextValue;
            }
        }
    }
}
