using Newtonsoft.Json;
using Sitecore.Commerce.Core;
using Sitecore.Services.Plugin.Sample.Converters;

namespace Sitecore.Services.Plugin.Sample.Policies
{
    [JsonConverter(typeof(PolicyConverter))]
    public class ExtendedPartyPolicy : Policy
    {
        public bool IsCompany { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
    }
}
