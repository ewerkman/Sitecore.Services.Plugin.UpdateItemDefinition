using Sitecore.Commerce.Engine.Connect.Entities;

namespace Sample.Commerce.Engine.Connect.Entities
{
    public class CustomParty : CommerceParty
    {
        public bool IsCompany { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
    }
}