﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddPartyCommand.cs" company="Sitecore Corporation">
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
    /// Defines the AddPartyCommand command.
    /// </summary>
    public class AddPartyCommand : CommerceCommand
    {
        protected CartCommander Commander { get; set; }

        public AddPartyCommand(CartCommander commander, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.Commander = commander;
        }

        public async Task<Cart> Process(CommerceContext commerceContext, string cartId, Party party)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var cart = await Commander.Command<FindEntityCommand>().Process(commerceContext, typeof(Cart), cartId) as Cart;

                if(cart != null)
                {
                    var additionalParties = cart.GetComponent<AdditionalPartiesComponent>();
                    additionalParties.AddParty(party);

                    await Commander.PersistEntity(commerceContext, cart);
                }

                return cart;
            }
        }
    }
}