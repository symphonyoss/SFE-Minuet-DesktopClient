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

echo "create exec zip"
cd minuet\symphony\bin
set relArchive=minuet-x86-release-%WRAPPER_VER%.zip
%zip% a -x!*.pdb -x!*app.publish -tZip %relArchive%
xcopy %relArchive% %targetsDir%
cd ..\..\..

echo "creating msi %proc%"

set pfxFile=Wrapper.Windows.Product.RSA.pkcs12.pfx
if NOT EXIST "%CEFDIR%\%pfxFile%" (
echo "can not find .pfx file" "%CEFDIR%\%pfxFile%"
exit /b -1
)

copy /y "%CEFDIR%\%pfxFile%" "%installerDir%\%pfxFile%"

cd minuet\symphony\Installer

set AIP=Minuet-x86

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

echo "copy result to target dir"
xcopy "%AIP%-SetupFiles\%AIP%.msi" %targetsDir% /i
set msiRelArchive=minuet-x86-release-%WRAPPER_VER%.msi
move "%targetDir%\%AIP%.msi" "%targetDir%\%msiRelArchive%"