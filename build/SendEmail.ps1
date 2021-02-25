#==============================================
# Seek on Build output dir for newly created EPiServer.Forms packages, and send information to recipients
#==============================================
Param(
	[Parameter(Mandatory=$true)]
    [string]$sourcePath,    # nuget packages source
	
	[Parameter(Mandatory=$true)]
    [string]$emailTo,    # recipients

    [string]$emailCC,

	# account for TeamCity to send mail
	[string]$emailAccount = "vnuser@episerver.com",
    [string]$emailPassword = "3PiS3rv3R!"

	)

$newlyCreatedPackages = Get-ChildItem -Path ..\nupkgs\* -Include *.nupkg  -Exclude *.symbols.nupkg,Alloy.Sample.ContentApi*.nupkg

$emailTemplate = "EmailTemplate.html"

Function SendReleaseEmail ([System.Collections.Generic.List[System.Object]]$packageList) {

	$combinedPackageNames = $packageList.ToArray() -join ", "
	$combinedPackageNamesWithoutExtension = $combinedPackageNames.Replace(".nupkg","")

	$Subject = "$combinedPackageNamesWithoutExtension have been built." 
	
	## Render Body content
	$Body = Get-Content $emailTemplate | ForEach-Object { 
		$_ -replace "{PackageNamesWithoutExtension}", $combinedPackageNamesWithoutExtension `
		-replace "{PackageNames}", $combinedPackageNames `
		-replace "{SourcePath}", $sourcePath 
	}
	  
	## Email settings
	$SMTPMessage = New-Object System.Net.Mail.MailMessage($emailAccount, $emailTo, $Subject, $Body)
	if ($emailCC) {$SMTPMessage.CC.Add($emailCC)}
	$SMTPMessage.IsBodyHTML = $true

	## Config email server
	$SMTPServer = "EPIEXCH01.ep.se" 

	$SMTPClient = New-Object Net.Mail.SmtpClient($SmtpServer, 25) 
	$SMTPClient.Credentials = New-Object System.Net.NetworkCredential($emailAccount, $emailPassword)
	$SMTPClient.UseDefaultCredentials = $false 
	$SMTPClient.Send($SMTPMessage)
}

#List of all published packages
$packageList= New-Object System.Collections.Generic.List[System.Object]

foreach($package in $newlyCreatedPackages) {
	$packageName = $package.Name

	#Skip packages not from Master or Release branch
	if (($packageName -match "-") -and !($packageName -match "-pre-")) {
		
		continue;
	}	
	else {
		$packageList.Add($packageName)
	}
}

#==================================Main================================== 
if ($packageList.Count -gt 0)
{
	$attempt = 5
	$success = $false	
	while ($attempt -gt 0 -and -not $success) {
		try {	
			SendReleaseEmail($packageList)
			$success = $true
		}
		catch {
			$attempt--
			Start-Sleep -s 60
		}
	}
}		