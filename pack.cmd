@echo off
setlocal

IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)
ECHO packing in %Configuration%

call npm run pack -- --configuration %Configuration%
IF %errorlevel% NEQ 0 EXIT /B %errorlevel%

REM Tell TeamCity to publish the artifacts even though the entire build isn't done
ECHO ##teamcity[publishArtifacts 'AddOnGit/ContentApi/nupkgs/*.nupkg']
IF %errorlevel% NEQ 0 EXIT /B %errorlevel%
