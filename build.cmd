@ECHO OFF
SETLOCAL

call npm install

REM Set Release or Debug configuration.
IF "%1"=="Debug" (set CONFIGURATION=Debug) ELSE (set CONFIGURATION=Release)
ECHO Building in %CONFIGURATION%

REM Set the build version. Using defaults when no params are given (common when running locally).
IF "%2"=="" ( SET BUILD=01 ) ELSE ( SET BUILD=%2 )
IF "%3"=="" ( SET BRANCH=developerbuild ) ELSE ( SET BRANCH=%3 )

CALL npm run build:setversion -- --build %BUILD% --jirabranch %BRANCH%
IF %errorlevel% NEQ 0 EXIT /B %errorlevel%

rem Build the C# solution.
dotnet build -c %CONFIGURATION%
IF %errorlevel% NEQ 0 EXIT /B %errorlevel%

