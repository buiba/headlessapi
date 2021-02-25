# Installing the Content Api

## Nuget Packages  ##

### Version 1.0.0 ###

The Content Delivery Api can be installed using the only one nuget package: EPiServer.ContentDeliveryApi

### From version 1.1.0 ###

The Content Delivery Api can be installed into existing Episerver solutions using the following Nuget packages:

```
* EPiServer.ContentDeliveryApi.Core
* EPiServer.ContentDeliveryApi.Cms
* EPiServer.ContentDeliveryApi.Search
* EPiServer.ContentDeliveryApi.OAuth
* EPiServer.ContentDeliveryApi.OAuth.UI
```

`EPiServer.ContentDeliveryApi.Cms` contains the api for Cms and the Site Definition Api.  
`EPiServer.ContentDeliveryApi.Search` contains the api for Search, and is dependent on Episerver Find.  
`EPiServer.ContentDeliveryApi.OAuth` contains an OAuth implementation for authenticating external applications with Episerver, and is dependent on ASP.NET Identity and Owin.  
`EPiServer.ContentDeliveryApi.OAuth.UI` contains supporting user interface elements for administering OAuth refresh tokens issued to users.  

## Installing the Content Api ##

### For the version 1.0.0 ### 
For this version, after installing EPiServer.ContentDeliveryApi, our infrastructure setting has not been recognized by Alloy Site, so to work around with this, we have to copy those files: SiteInitialization.cs, StructureMapDependencyScope.cs, StructureMapResolver.cs, and StructureMapDependencyResolver.cs in folder Infrastructure in EPiServer.ContentApi.MusicFestival.Site to the site.


Note: those files' namespace can be changed.

### From the version 1.1.0 ###
After installing the `EPiServer.ContentDeliveryApi` package, it must be initialized from within an IConfigurableModule, and CORS and Attribute Routing must be enabled within Web Api in order to support all features:

```
public void ConfigureContainer(ServiceConfigurationContext context)
{
    ...

    context.InitializeContentApi();

    ...

    GlobalConfiguration.Configure(config =>
    {
        ...

        config.MapHttpAttributeRoutes();
        config.EnableCors();
    });
}

```

The Content Delivery Api can be configured from here by passing in an instance of `ContentApiOptions`, allowing the following elements of the Api to be configured:

### Minimum Roles ###

By default, the Content Delivery Api is accessible by anonymous users, and only returns content that users are authorized to see. In some situations, you may want to limit access to a certain set of roles. The `MinimumRoles` property allows a comma-separated list of role names to be provided, and only users with these roles will be able to access any operation of the Content Delivery Api.

### Required Role ###

By default, the Content Delivery Api will only return content that has been granted an explicit Required Role via the Episerver Access Rights system. This security feature enables developers to selectively expose certain content for traversal in the Api, while leaving other content for internal use. The role which must be granted access is named `Content Api Access`, but this can be changed by adjusting the `RequiredRole` setting on `ContentApiOptions`. 

In order to expose content in the Api, give the Required Role (`Content Api Access`) Read permission via the Episerver Edit interface, or via the "Set Access Rights" page in the Episerver Admin section. Once granted permission, this content will return in requests across the Content Api. If the Content Search Api is being utilized, there may be a delay in the content being exposed as Episerver Find processes re-indexing events. 

In order to disable this functionality entirely, set the `RequiredRole` property to `null`, or `Everyone`. 

When using the Required Role feature, Content will still be filtered based on the access rights of the requesting user, ensuring that access rights set on content are enforced as expected. For example, if a page is only accessible by `Administrators`, but has been exposed in the Content Api via the `Required Role` being granted Read permission, it will still only be accessible in the Content Api by users who are `Administrators`.

Note: The Content Delivery Api does not filter out content in `ExpandedValue` properties based on the Required Role. If a content item (like an Image or Block) is referenced in a Content Reference or Content Area, it will be exposed in the Content Api when the property is expanded. This is by design, to ensure the Api is convenient to navigate related content.

### Clients ###

The Content Delivery Api supports multiple client applications connecting to the Api via the `Clients` property. By default, a single Client is configured, with the ClientID of `Default`. Clients enable support for two concepts: CORS and Authorization

#### CORS ####

Each client supports an individual configuration for Cross-Origin Resource Sharing (CORS), via the `AccessControlAllowOrigin` property, which restricts the domains that can call the Content Delivery Api when requests are made within the browser from different domains. The `Default` client is configured to support all origins via a wildcard configuration - `*`. By passing in a single origin, such as `https://www.episerver.com`, requests made from within browsers must originate from that domain in order to succeed. NOTE: CORS does not apply to requests from the same origin.

In the event that multiple Clients are configured, all Clients must either utilize a wildcard configuration, or must specify individual origins - these configurations cannot be mixed. Individual origins are not associated directly with a Client in the Content Api, and thus Client Ids do not need to be attached to each individual request, except in requests to the Authorization endpoint - see [Authorization](Authorization.md) for more information.

#### Authorization ####

If the `EPiServer.ContentApi.OAuth` package is in use for authenticating users with the Content Api, Clients provide a separation between access and refresh tokens issued to users. For more information, see [Authorization](Authorization.md).

### Multi-site Filtering ###

By default, only information and content from the current Episerver site is returned in requests to the Content Api. The current site is detected based on the request context, and configured Sites and their domains in the Episerver Admin panel.

This feature ensures proper separation between sites in a multi-site configuration and extends across the Content Api, the Content Search Api, and the Site Definition Api. In order to disable this option, set the `MultiSiteFilteringEnabled` property to `false`.

Note: The Content Api does not filter out content in `ExpandedValue` properties based on Multi-site Filtering. If a content item (like an Image or Block) is referenced in a Content Reference or Content Area, it will be exposed in the Content Api when the property is expanded. This is by design, to ensure the Api is convenient to navigate related content.

For more information, see [Site Definition Api](SiteDefinitionApi.md), and [Content Api](Content.md), and the [Content Search Api](Search.md) documentation.

### Site Definition Api ###

In order to disable the Site Definition Api, the `SiteDefinitionApiEnabled` parameter can be set to `false`, which results in the Site Definition Api returning a 404. For more information, see [Site Definition Api](SiteDefinitionApi.md).

## Installing the Content Search Api ##

After installing the `EPiServer.ContentApi.Search` package, it must be initialized from within an IConfigurableModule, just after the Content Api is initialized:

```
public void ConfigureContainer(ServiceConfigurationContext context)
{
    ...
    context.InitializeContentApi();
    context.InitializeContentSearchApi();
}

```

Once initialized, The EPiServer Find Content Indexing Job must be run in order to index the models used by the Content Search Api into Find.

The Content Search Api can be configured from here by passing in an instance of `ContentSearchApiOptions`, allowing the following elements of the Api to be configured:

### Search Cache Duration ###

By default, The Content Search Api caches all requests to Episerver Find for 30 minutes. Using the `SearchCacheDuration` property, this TimeSpan can be customized to limit how long results will be cached. In order to disable caching, set the property to TimeSpan.Zero. 

### Maximum Search Results ###

By default, the Content Search Api returns a maximum of 100 results in a given request. If the search request is passed with a `top` parameter larger than this maximum, a `400 Bad Request` status code will be returned. The `MaximumSearchResults` parameter allows this limit to be decreased, or increased, and developers should be cognizant of performance considerations when adjusting this value.
