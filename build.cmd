@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

SET PATH=%SystemRoot%\Sysnative;%PATH%
FOR /F "tokens=2*" %%A IN ('REG.EXE QUERY "HKLM\Software\GitForWindows" /V "InstallPath" 2^>NUL ^| FIND "REG_SZ"') DO SET GITPATH=%%B

"%GITPATH%\bin\git" -C %CD% rev-list HEAD --count > tmp.tmp 2>nul
set /p REV_COUNT=<tmp.tmp

"%GITPATH%\bin\git" -C %CD% log -1 --date=iso-local --pretty=format:%%cd > tmp.tmp
set /p REV_DATE=<tmp.tmp

"%GITPATH%\bin\git" -C %CD% log -1 --pretty=format:%%h --abbrev=8 > tmp.tmp
set /p REV_HASH=<tmp.tmp

"%GITPATH%\bin\git" -C %CD% config --get remote.origin.url > tmp.tmp
set /p REV_URL=<tmp.tmp

"%GITPATH%\bin\git" -C %CD% describe --tags > tmp.tmp 2>nul
set /p REV_TAG=<tmp.tmp
if !REV_TAG!.==. set REV_TAG=(no tag)

"%GITPATH%\bin\git" -C %CD% symbolic-ref --short HEAD > tmp.tmp
set /p REV_BRANCH=<tmp.tmp

if exist tmp.tmp del tmp.tmp

"%GITPATH%\bin\git" -C %CD% diff --exit-code --quiet
if %errorlevel% == 0 (
  set REV_DIRTY=
) else (
  set REV_DIRTY=M
)

set APP_REV=%REV_DIRTY%%REV_COUNT%-%REV_HASH%

dotnet publish "ScanApp\ScanApp.csproj" -c Release /p:PublishProfile=Windows /p:PublishDir=../publish/win-x64 ^
	/p:Description="Built from commit !REV_HASH! in !REV_BRANCH! on !REV_DATE! tag !REV_TAG!" ^
	/p:VersionSuffix="!REV_DIRTY!!REV_COUNT!-!REV_HASH!"

dotnet publish "ScanApp\ScanApp.csproj" -c Release /p:PublishProfile=Linux /p:PublishDir=../publish/linux-x64 ^
	/p:Description="Built from commit !REV_HASH! in !REV_BRANCH! on !REV_DATE! tag !REV_TAG!" ^
	/p:VersionSuffix="!REV_DIRTY!!REV_COUNT!-!REV_HASH!"

: dotnet publish "ScanApp\ScanApp.csproj" -c Release /p:PublishProfile=MacOS /p:PublishDir=../publish/osx-x64 ^
:	/p:Description="Built from commit !REV_HASH! in !REV_BRANCH! on !REV_DATE! tag !REV_TAG!" ^
:	/p:VersionSuffix="!REV_DIRTY!!REV_COUNT!-!REV_HASH!"
