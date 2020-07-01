using System.Linq;
using Commerce.Plugin.Sample.Payment.Components;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Sample.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Engine.Connect.Entities;
using Sitecore.Commerce.Engine.Connect.Pipelines.Arguments;
using Sitecore.Commerce.Engine.Connect.Pipelines.Carts;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Payments;
using Sitecore.Services.Plugin.Sample.Components;
using Sitecore.Services.Plugin.Sample.Policies;

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

            this.TranslateCustomPaymentMethods(source, customCart);
            this.TranslateAdditionalParties(source, customCart);
        }

        private void TranslateCustomPaymentMethods(Cart source, CustomCart customCart)
        {
            var payments = source.Components.OfType<PaymentComponent>().ToList();
            if (!payments.Any())
            {
                return;
            }

            foreach (var payment in payments)
            {
                if (payment is SimplePaymentComponent simplePayment)
                {
                    var simplePaymentInfo = this.EntityFactory.Create<SimplePaymentInfo>("SimplePayment");
                    simplePaymentInfo.PaymentMethodID = simplePayment.PaymentMethod.EntityTarget;
                    simplePaymentInfo.Amount = simplePayment.Amount.Amount;
                }
            }

        }

        private void TranslateAdditionalParties(Cart source, CustomCart customCart)
        {
            var additionalParties = source.Components.OfType<AdditionalPartiesComponent>().FirstOrDefault();
            additionalParties?.Parties.ForEach(party =>
            {
                var extendedPartyPolicy = party.Policies.OfType<ExtendedPartyPolicy>().FirstOrDefault();
                customCart.CustomParties.Add(new CustomParty()
                {
                    FirstName = party.FirstName,
                    LastName = party.LastName,
                    Address1 = party.Address1,
                    Address2 = party.Address2,
                    City = party.City,
                    Country = party.Country,
                    State = party.State,
                    ZipPostalCode = party.ZipPostalCode,
                    IsCompany = extendedPartyPolicy?.IsCompany ?? false,
                    Gender = extendedPartyPolicy?.Gender,
                    Title = extendedPartyPolicy?.Title,
                    Phone = extendedPartyPolicy?.Phone
                });
            });
        }
    }
}