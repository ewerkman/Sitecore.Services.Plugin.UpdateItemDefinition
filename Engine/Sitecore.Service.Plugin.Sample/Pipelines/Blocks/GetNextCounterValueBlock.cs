using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Services.Plugin.Sample.Entities;
using Sitecore.Services.Plugin.Sample.Pipelines.Arguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sitecore.Services.Plugin.Sample.Pipelines.Blocks
{
    public class GetNextCounterValueBlock : PipelineBlock<GetNextCounterValueArgument, long, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Gets or sets the commander.
        /// </summary>
        /// <value>
        /// The commander.
        /// </value>
        protected CommerceCommander Commander { get; set; }
        
        //Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        /// <inheritdoc />
        /// <summary>Initializes a new instance of the <see cref="T:Sitecore.Framework.Pipelines.PipelineBlock" /> class.</summary>
        /// <param name="commander">The commerce commander.</param>
        public GetNextCounterValueBlock(CommerceCommander commander)
            : base(null)
        {
            this.Commander = commander;
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="arg">
        /// The pipeline argument.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The <see cref="PipelineArgument"/>.
        /// </returns>
        public override async Task<long> Run(GetNextCounterValueArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");
            await semaphoreSlim.WaitAsync();
            try
            {
                var entityId = $"{CommerceEntity.IdPrefix<Counter>()}{arg.CounterName}";

                var counter = await Commander.GetEntity<Counter>(context.CommerceContext, entityId);

                if (counter == null)
                {
                    context.Abort(
                          await context.CommerceContext.AddMessage(
                              context.GetPolicy<KnownResultCodes>().ValidationError,
                              "CounterNameAlreadyInUse",
                              new object[] { arg.CounterName },
                              $"Counter name {arg.CounterName} could not be found.").ConfigureAwait(false),
                          context);

                    return -1;
                }

                long nextCounterValue = -1;


                nextCounterValue = counter.GetNextValue();
                await Commander.PersistEntity(context.CommerceContext, counter);


                return nextCounterValue;
            }
            catch (Exception ex)
            {
                context.CommerceContext.LogException(this.Name, ex);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                semaphoreSlim.Release();
            }

            return -1;
        }
    }
}
