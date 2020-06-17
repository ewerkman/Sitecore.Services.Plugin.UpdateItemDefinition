using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Sample.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Engine.Connect.Pipelines.Arguments;
using Sitecore.Commerce.Engine.Connect.Pipelines.Carts;
using Sitecore.Commerce.Entities;
using Sitecore.Services.Plugin.Sample.Components;

namespace Sample.Commerce.Engine.Connect.Pipelines.Carts
{
    public class TranslateCustomCartToEntity : TranslateCartToEntity
    {
        public TranslateCustomCartToEntity(IEntityFactory entityFactory) : base(entityFactory)
        {
        }

        protected override void Translate(TranslateCartToEntityRequest request, Sitecore.Commerce.Plugin.Carts.Cart source, CommerceCart destination)
        {
            base.Translate(request, source, destination);

            var customCart = destination as CustomCart;

            var additionalParties = source.Components.OfType<AdditionalPartiesComponent>().FirstOrDefault();
            additionalParties?.Parties.ForEach( party => customCart.CustomParties.Add(new CommerceParty() {
                    FirstName = party.FirstName,
                    LastName = party.LastName,
                    Address1 = party.Address1,
                    Address2 = party.Address2,
                    City = party.City,
                    Country = party.Country,
                    State = party.State,
                    ZipPostalCode = party.ZipPostalCode
                })
            );
        }
    }
}