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

dotnet restore "ScanApp.csproj"
dotnet build "ScanApp.csproj" ^
	-p:DeployOnBuild=true ^
	-p:PublishProfile="Properties/PublishProfiles/Windows.pubxml" ^
	-p:Description="Built from commit !REV_HASH! in !REV_BRANCH! on !REV_DATE! tag !REV_TAG!" ^
	-p:VersionSuffix="!REV_DIRTY!!REV_COUNT!-!REV_HASH!"

dotnet build "ScanApp.csproj" ^
	-p:DeployOnBuild=true ^
	-p:PublishProfile="Properties/PublishProfiles/Linux.pubxml" ^
	-p:Description="Built from commit !REV_HASH! in !REV_BRANCH! on !REV_DATE! tag !REV_TAG!" ^
	-p:VersionSuffix="!REV_DIRTY!!REV_COUNT!-!REV_HASH!"

dotnet build "ScanApp.csproj" ^
	-p:DeployOnBuild=true ^
	-p:PublishProfile="Properties/PublishProfiles/MacOS.pubxml" ^
	-p:Description="Built from commit !REV_HASH! in !REV_BRANCH! on !REV_DATE! tag !REV_TAG!" ^
	-p:VersionSuffix="!REV_DIRTY!!REV_COUNT!-!REV_HASH!"
