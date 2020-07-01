// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeletePartyCommand.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Services.Plugin.Sample.Commands.Parties
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Services.Plugin.Sample.Components;
    using System;
    using System.Threading.Tasks;

    /// <inheritdoc />
    /// <summary>
    /// Defines the DeletePartyCommand command.
    /// </summary>
    public class RemovePartyCommand : CommerceCommand
    {
        protected CommerceCommander Commander { get; set; }

        public RemovePartyCommand(CommerceCommander commander, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.Commander = commander;
        }

        public async Task<CommerceEntity> Process(CommerceContext commerceContext, string cartId, string addressName)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var cart = await Commander.Command<FindEntityCommand>().Process(commerceContext, typeof(Cart), cartId) as Cart;

                if (cart != null)
                {
                    var additionalParties = cart.GetComponent<AdditionalPartiesComponent>();
                    additionalParties.RemoveParty(addressName);

                    await Commander.PersistEntity(commerceContext, cart);
                }

                return cart;
            }
        }
    }
}