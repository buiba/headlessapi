Param(
    $SiteName,
    $PackageName,
    $PackageVersion,
    [bool]$DeleteSite = $true,
    [bool]$CreateSite = $true
)

$SiteRoot = "c:\contentapi"
$IISSiteName = "Default Web Site\ContentApi"

$SiteName = $SiteName -replace "\W+", "-"
$SitePath = "$SiteRoot\$SiteName"
"SiteName: $SiteName"
"SiteRoot: $SiteRoot"

$tmpFolder = [System.IO.Path]::GetTempPath() + [guid]::NewGuid().ToString()
$tmpPackageFolder = "$tmpFolder\$PackageName.$PackageVersion"

#http://www.iis.net/learn/manage/powershell/powershell-snap-in-creating-web-sites-web-applications-virtual-directories-and-application-pools
Import-Module WebAdministration
Import-Module 'C:\DailySiteItems\MSBuildTools\EPiServerInstall.Common.1.dll'

function Delete-Site {
    param (
            $SiteName,
            $SitePath
        )

    $site = "IIS:\Sites\$IISSiteName\$SiteName"
    $appPoolName = "IIS:\AppPools\$SiteName"

    if(Test-Path ($appPoolName)) {
        "Stopping $appPoolName"
        Stop-WebItem $appPoolName -Passthru
    }

    Function DeleteIfExists($path) {
        if(Test-Path($path)) {
            "Deleting: $path"
            Remove-Item $path -Recurse
        }
    }

    DeleteIfExists $site
    DeleteIfExists $appPoolName
    DeleteIfExists $SitePath
}

function Create-Site {
    param (
            $SiteName,
            $SitePath
        )


    $site = "IIS:\Sites\$IISSiteName\$SiteName"
    $appPoolName = "IIS:\AppPools\$SiteName"

    if(!(Test-Path($SitePath))) {
		#Create folder for current branch in server
        "Creating folder: $SitePath"
        md $SitePath
        "Creating folder: $SitePath\AppData"
        md "$Sitepath\AppData"
    }

    if(!(Test-Path($site))) {
        "Creating site: $site"
        New-Item $site -type Application -physicalPath "$SitePath\wwwroot"

        if(!(Test-Path($appPoolName)))
        {
            "Creating Application pool: $appPoolName"
            $appPool = New-Item ($appPoolName)
            $appPool.managedRuntimeVersion = "v4.0"
            $appPool | Set-Item
        }

        #Set the app pool on the site
        "Set $appPoolName as the application pool for $site"
        Set-ItemProperty $site -name applicationPool -value $SiteName
    } else {
        "Site $site already exists"
    }
}

function Transform-Config {
    param (
        $TmpPackageFolder,
        $SitePath,
        $DbServer,
        $SiteName,
        $DbSiteUser,
        $DbSitePassword
    )

    "Transform config files"
    Update-EPiXmlFile -TargetFilePath "$SitePath\wwwroot\Web.config" -ModificationFilePath "$TmpPackageFolder\Setup\Alloy.Sample.ContentApi.web.config.transform" -Replaces "{basePath}=$SitePath\appData;"
}

function Deploy-Nuget {
    param (
            $TmpFolder,
            $TmpPackageFolder,
            $PackageName,
            $PackageVersion,
            $SitePath
        )

    $tmpPackageFolderSearch = "$TmpPackageFolder\*"

    #Install the nuget package into the $tmpFolder
    nuget install $PackageName -Version $PackageVersion -Prerelease -OutputDirectory $tmpFolder -NoCache

    "Copy the site from $tmpPackageFolderSearch to $SitePath"
    Copy-Item $tmpPackageFolderSearch -Destination $SitePath -Recurse
}

if ($DeleteSite -eq $true) {
    Delete-Site $SiteName $SitePath
}

if ($CreateSite -eq $true) {
    Create-Site  $SiteName $SitePath
    Deploy-Nuget $tmpFolder $tmpPackageFolder $PackageName $PackageVersion $SitePath
    Transform-Config $tmpPackageFolder $SitePath $DbServer $SiteName $DbSiteUser $DbSitePassword

    "Remove temp folder $tmpFolder"
    Remove-Item $tmpFolder -Recurse
}
