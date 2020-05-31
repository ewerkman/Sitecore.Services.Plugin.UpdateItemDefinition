using Sitecore.Commerce.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Services.Plugin.Sample.Components
{
    public class AdditionalPartiesComponent : Component
    {
        public AdditionalPartiesComponent()
        { 
        }

        public List<Party> Parties { get; internal set; } = new List<Party>();

        public Party AddParty(Party party)
        {
            // Remove the party based on addressname if it already exists
            Parties.RemoveAll(p => p.AddressName == party.AddressName);
            Parties.Add(party);
        
            return party;
        }

        public void RemoveParty(string addressName)
        {
            Parties.RemoveAll(p => p.AddressName == addressName);
        }
    }
}
