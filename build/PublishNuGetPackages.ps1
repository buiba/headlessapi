#==============================================
#	Publish nuget package from Git
#	Seek on Build output dir and copy newly created EPiServer.ContentApi nuget package to $destinationPath
#==============================================
Param(
    [Parameter(Mandatory=$true)]
    [string]$destinationPath
	)
	
if (!$destinationPath) {
    Write-Error "destinationPath must be supplied"
    exit 1
}

$packagesToDeploy = Get-ChildItem -Path ..\nupkgs\* -Include *.nupkg  -Exclude *.symbols.nupkg,Alloy.Sample.ContentApi*.nupkg

# Create Directory if not exist!!!
#if (!(Test-Path -path $destinationPath)) {New-Item $destinationPath -Type Directory}

foreach($package in $packagesToDeploy) {
	$packageName = $package.Name
	
	#Skip packages not from Master or release branch
	if (($packageName -match "-") -and !($packageName -match "-pre-")) {
		continue;
	}	
	
	#Skip Symbols packages
	if (($packageName -match "symbols")) {
		continue;
	}
	
	$packageFullName = $package.FullName

	#Echo some information to console
	"Publishing package: $packageName"
	"Path: $packageFullName"
	
	Copy-Item -Path $packageFullName -Destination "$destinationPath" -Force -Recurse 
}