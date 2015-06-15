@echo off

set proc=%1

rem AdvancedInstaller.com must be in PATH env

if NOT DEFINED WRAPPER_VER (
	echo "environment variable WRAPPER_VER not defined"
	exit /b -1
)

echo "creating targets directory"
rmdir /q /s targets
mkdir targets
set targetsDir="%CD%\targets\"

rem tools dependencies, must be in your path
set zip=7z.exe

set archiveName=DesktopClient-Win-x86-%WRAPPER_VER%

echo "create exec zip"
cd minuet\symphony\bin
set zipArchive=%archiveName%.zip
%zip% a -x!*.pdb -x!*app.publish -tZip %zipArchive%
xcopy %zipArchive% %targetsDir%
cd ..\..\..

echo "creating msi %proc%"

set installerDir=%CD%\minuet\symphony\Installer

set pfxFile=Wrapper.Windows.Product.RSA.pkcs12.pfx
if NOT EXIST "%CEFDIR%\%pfxFile%" (
echo "can not find .pfx file" "%CEFDIR%\%pfxFile%"
exit /b -1
)

copy /y "%CEFDIR%\%pfxFile%" "%installerDir%\%pfxFile%"

cd %installerDir%

set AIP=Symphony-x86

if EXIST %AIP%-cache (
	echo "remove old msi cache file"
	rmdir /q /s %AIP%-cache
)
if EXIST %AIP%-SetupFiles (
	echo "remove old msi setup files"
	rmdir /q /s %AIP%-SetupFiles
)

echo "running advanced installer to build msi"

AdvancedInstaller.com /edit %AIP%.aip /SetVersion %WRAPPER_VER%
IF %errorlevel% neq 0 (
	echo "failed to set advanced installer build version"
	exit /b -1
)

AdvancedInstaller.com /build %AIP%.aip
IF %errorlevel% neq 0 (
	echo "error returned from advanced installer:" %errorlevel%
	exit /b -1
)

if NOT EXIST %AIP%-SetupFiles/%AIP%.msi (
	echo "failure did not produce %proc% msi"
	exit /b -1
)

echo "copy msi result to target dir"
copy "%AIP%-SetupFiles\%AIP%.msi" "%targetsDir%\%archiveName%.msi"