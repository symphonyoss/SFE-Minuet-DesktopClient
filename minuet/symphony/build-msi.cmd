cd Installer
set AIP=Minuet-x86

set pfx=Wrapper.Windows.Product.RSA.pkcs12.pfx
copy "%CEFDIR%\%pfx%" %pfx%

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
