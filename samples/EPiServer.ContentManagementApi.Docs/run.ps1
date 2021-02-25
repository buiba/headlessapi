Install-Module Newtonsoft.Json
Import-Module Newtonsoft.Json

#ensure site builds
Invoke-Expression "dotnet build"

#start docs site
$iisExpressExe = '"c:\Program Files (x86)\IIS Express\iisexpress.exe"'
$path = (Resolve-path .)
$port = 53035
Write-host "Starting site from path: $path on port: $port"
$proc = Start-Process -FilePath $iisExpressExe -ArgumentList "/port:$port","/path:$path" -PassThru  
Start-Sleep -m 1000
Write-Host "Site started"

#update json file
$raw = (Invoke-WebRequest "http://localhost:$port/swagger/docs/2-18").Content
$pretty = $raw | ConvertFrom-JsonNewtonsoft | ConvertTo-JsonNewtonsoft
$swaggerFile = Join-Path -Path $path -ChildPath "swagger-doc.json"
Set-Content -Path $swaggerFile -Value $pretty
Write-Host "updated swagger definition file: " $swaggerFile

#cleanup
$proc.Kill()

