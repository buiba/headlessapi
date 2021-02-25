Episerver Content Delivery Api 
===========

The Episerver Content Delivery Api is a set of Apis for accessing IContent over HTTP.  It's designed to allow Episerver content to be leveraged in external applications, such as Mobile Apps and Connected Devices, and it also aims to support client-side Javascript development when building Episerver sites.

It consists of four separate packages which can be installed and configured on existing Episerver installations, in a variety of configurations:

* EPiServer.ContentDeliveryApi.Core
* EPiServer.ContentDeliveryApi.Cms
* EPiServer.ContentDeliveryApi.Search
* EPiServer.ContentDeliveryApi.OAuth
* EPiServer.ContentDeliveryApi.OAuth.UI
* EPiServer.ContentDeliveryApi.Form
* EPiServer.ContentDeliveryApi.Commerce


Swagger API Documentation can be found [here](docs/design/).

## Quick Start ##
* [Installation](docs/Installation.md)
* [Authorization & Access Control](docs/Authorization.md)
* [Content Delivery Api Cms](docs/Content.md)
* [Content Delivery Api Search](docs/Search.md)
* [Site Definition Api](docs/SiteDefinition.md)

## Advanced Topics ##
* [Serialization](docs/Serialization.md)
* [URL Handling](docs/URLs.md)

## Building Content Api on Team City

ContentDeliveryApi has the following builds (based on the same template). The build compiles, run tests, signs and create nuget packages. This means every checkin will produce valid nuget packages.

* ContentDeliveryApi_CI: Runs automatically on all branches except `master` and `release/*`. Publishes the inte- and feature- nuget packages to nuget.ep.se.
* ContentDeliveryApi_Release: Should be ran manually on master and `release/*` branches. Publishes nuget packages to nuget.ep.se.

All DLL's and nuget-packages are pre-release tagged except the packages built from master.

Nuget packages and assemblies are tagged automatically in the builds according to this:

* develop are named `2.12.1001.0-inte-XXXX` where `XXXX` is the build number
* release branches `2.12.1001.0-pre-XXXX`
* feature branches `2.12.1001.0-feature-HAPI-531-XXXX` where `HAPI-531` will match the jira id from the feature branch name: `feature/HAPI-531-my-feature`.
* all other `2.12.1001.0-developerbuild-XXXX`

### Publishing nuget packages to internal nuget server

As mentioned above, the `master` and `release/*` branches will automatically publish its nuget packages to nuget.ep.se. 
If you want packages from another branch available on the nuget server you can manually queue a build of your branch in either the `ContentDeliveryApi_Release` or the `ContentDeliveryApi_CI` build definition with the publishPackages argument is set to `true`

## Creating a release

* In Git pull develop and master to make sure you have the latest code.
* Create a new branch from _develop_.
* Name branch after _expected_ version number of the new release, for example "release/2.8.0".
* commit/push this branch.
* Builds will trigger automatically and automatically updates Jira.

Note: Version numbers are _only_ changed on release branches.

## Finishing a release

* In Git, pull release-branch(ie release/2.8.0), develop and master to make sure you have the latest code.
* Merge release-branch to master.
* Tag the latest commit on master with vX.X.X (ie v2.8.0)
* Merge master to develop
* Push master and develop (including tags!)
* Builds will trigger automatically and automatically update Jira.
* Delete older release branches (release/* should only contain the last released version, the tag is used to track versions)
* Copy packages from \\\\epdevsource\NuGet to \\\\t3\\Releases\\thisweek


## Changing code in this repo

* Create branch from item in Jira
* For bugfixes add a unit test to catches the bug before continuing
* Check coverage on integration tests, if there are no integration tests on the area you are planning to code they must be written first
* Write tests, code, write tests, code  :)
* Make sure you have the latest code from develop
* Make sure builds are green, AddOns CI runs on every checkin
* Create a pull requests from your branch to develop and add reviewers

## Write a commit message

https://wiki.episerver.net/display/COM/Commit+Message+Guidelines

## Branching model

We are using the workflow as described in https://www.atlassian.com/git/workflows#!workflow-gitflow with some minor modifications.

### Master branch

Should always contain tested, working, releasable code. You can only get code onto master via merging tested releases.

### Develop branch

Acts as integration branch for feature and bugfix branches that should go into the next release.
You can only get code onto develop by creating pull requests that then needs to be reviewed and accepted. Should only contain completely implemented work items, but code here might not have been thru QA.

### Feature branches

Created from develop and should be named feature/<key in Jira>-<short description>. Create branch from Jira to get correct naming.
Merge to the develop branch by creating a pull request.

### Bugfix branches

Created from develop and should be named bugfix/<key in Jira>-<short description>. Create branch from Jira to get correct naming.
Merge to the develop branch by creating a pull request.

### Release branches

Created from develop to send the code to QA, the changes are then merged to master and back to develop. This is the only place we change version number.

## Setup development environment

To set up a development environment, run:

```
setup.cmd
```
This task will copy the database, add a template episerverdata file to the Alloy sample

## Daily sites

When a branch is built a test site is automatically deployed to http://addons-daily.ep.se/ContentApi/
The following users are available for logging onto the sites. 

|Name    |Password   
|--------|------------|
|cmsadmin|sparr0wHawk 
|emil    |sparr0wHawk 
|ida     |sparr0wHawk 
|alfred  |sparr0wHawk 
|lina    |sparr0wHawk

NOTE: Since the daily site uses PackageReference and does not support automatic copy content files when installing the NuGet package. If we want to update the CMS.UI package for the Alloy site to a newer version, we MUST manually copy the corresponding CMS.UI content files (i.e modules/protected/CMS.zip, ...) to the daily site root folder. Otherwise, the 'All properties' mode in EditView will not work.

## Creating nuget packages in local

To set up a development environment, run:

```
build.cmd
```
then run
```
pack.cmd
```

This task will create nuget packages then put them into nupkgs folder

## Known Issues ##

* URLs retrieved from the Content Api are not able to be generated in Edit or Preview contexts for proper use in Edit Mode navigation when links are clicked
* Strongly-typed properties which have custom getter methods (such as fallbacks to other properties) are not reflected in the Api
* XhtmlString properties do not properly render the output of Blocks which have been dragged in when indexed into Find, so these are filtered out in the version indexed into Find. These properties are rendered properly when retrieved directly from the database.
* OData filter syntax does not support filtering on base property collections on ContentApiModel, such as the `ExistingLanguages` property. Collections in user-defined properties are properly filterable.


### Known Issues - INTERNAL NOTES ###

* Performance Consideration: DefaultPropertyModelHandler does not cache mappings between PropertyData types and TypeModel classes, causing repeated iteration
* Performance Consideration: DefaultPropertyModelHandler uses standard .NET API for reflection, which may be less efficient / fast than some FOSS alternatives (FastMember, Festerflect, HyperDescriptor)
* Performance Consideration: Content in Find is always indexed with the ContentApiModel property attached, even if the content does not have the configured RequiredRole associated with it.
* All Api methods do not set `Cache-Control` headers when responses may be cachable (response is always `no-cache`)
* Detailed logging (DEBUG / INFO) level is not in place within all layers of the Api
* Content Api methods do not explicitly filter out Commerce content, but it is never returned since it does not implement `IContentSecurable` yet and all operations enforce Access Rights properly. Access Rights support was added in Commerce 11.6, so Commerce content will either need to be explicitly filtered out, or support will need to be added to allow Commerce content to be exposed, along with associated standard Commerce properties.
