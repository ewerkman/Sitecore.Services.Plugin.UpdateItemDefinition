
## FilterFulfillmentMethodsBlock

When you call `GetCartLineFulfillmentMethods()` this block will look at the country code delivery address and only return the fulfillment methods that are valid for this address. 

For this to work, you need to create an extra template that you will use to configure the countries the fulfillment method is valid for.

Create a new template that contains a field called `Shipping Countries` of type `Multilist` and Source `/sitecore/Commerce/Commerce Control Panel/Shared Settings/Countries-Regions`. This will let you select the countries for a fulfillment method.

Next let the Shipping Method template (`/sitecore/templates/CommerceConnect/Sitecore Commerce/Commerce Control Panel/Shared Settings/Fulfillment/Fulfillment Method`) inherit from the template you just created. Now if you go to the fulfillment methods (e.g. `/sitecore/Commerce/Commerce Control Panel/Shared Settings/Fulfillment Options/Ship items`) you should see it is extended with a section where you can select the shipping countries.

Now you can select the shipping countries for each shipping method. Note that if you don't select a country, the fulfillment method will never be available.