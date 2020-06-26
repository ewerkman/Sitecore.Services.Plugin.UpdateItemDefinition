using Sitecore.Commerce.Core;
using Sitecore.Services.Plugin.Sample.Policies;
using System;

namespace Sitecore.Services.Plugin.Sample.Converters
{
    /// <summary>
    ///     Configure Newtonsoft.Json to use this converter by adding it to the Commerce.Engine project 
    ///     in the constructor of the Startup class:
    ///     
    ///      JsonConvert.DefaultSettings = (() =>
    ///        {
    ///            var settings = new JsonSerializerSettings();
    ///            settings.Converters.Add(new PolicyConverter());
    ///            return settings;
    ///        });
    /// </summary>
    public class PolicyConverter : ODataCreationConverter<Policy>
    {
        protected override Policy Create(Type objectType, string type)
        {
            switch (type)
            {
                case "Sitecore.Services.Plugin.Sample.Policies.ExtendedPartyPolicy":
                    return new ExtendedPartyPolicy();

                default:
                    return new Policy();
            }
        }
    }
}
