using System.Collections.Generic;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Engine.Connect.Entities;

namespace Sample.Commerce.Engine.Connect.Entities
{
    public class CustomCart : CommerceCart
    {
        public List<CommerceParty> CustomParties { get; set; } = new List<CommerceParty>();
    }
}