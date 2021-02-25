Param(
       
    #The Id (should be a guid) of the project.
	$ProjectId,
	
	#The client key used to access the project.
    $ClientKey,
	
	#The client secret used to access the project.
    $ClientSecret
)

if (!( Get-InstalledModule -Name "EpiCloud" -RequiredVersion 0.8.8)) {
	 Install-Module -Name EpiCloud -Force
}

$Global:EpiCloudApiEndpointUri = 'https://paasportal.epimore.com/api/v1.0/'

#Connect to Episerver Cloud
Connect-EpiCloud -ClientKey $ClientKey -ClientSecret $ClientSecret -ProjectId $ProjectId

$packageFile = Get-ChildItem -Path .\nupkgs\Alloy.ContentDelivery.cms.app.*.nupkg

#Get deploymentpackage location 
$packageLocation = Get-EpiDeploymentPackageLocation 

#Upload nuget package to DXP project
Add-EpiDeploymentPackage -SasUrl $packageLocation -Path $packageFile.FullName

#Deploy package to the Integration environment
$startDeploymentResponse = Start-EpiDeployment -DeploymentPackage $packageFile.Name -TargetEnvironment Integration -Wait  -Verbose

#If status of deployment is AwaitingVerification then complete deployment
if($startDeploymentResponse.status -eq  'AwaitingVerification'){
	Complete-EpiDeployment -Id $startDeploymentResponse.id
}
else{
	#Reset the deployment
	Reset-EpiDeployment -Id 2a52c873-b39c-4f44-b842-aab5009c3060 -Wait
	throw "Error when deploying application"
}