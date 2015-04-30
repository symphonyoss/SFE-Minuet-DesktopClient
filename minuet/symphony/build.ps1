echo "go to folder"
cd minuet
cd symphony

echo "Set Visual Studio env vars"
$path1='c:\Program Files (x86)\Microsoft Visual Studio 12.0\VC'
$path2='c:\Program Files\Microsoft Visual Studio 12.0\VC'
if ((Test-Path $path1) -eq $True) { pushd $path1 } 
elseif ((Test-Path $path2) -eq $True) { pushd $path2 } 
else { 
    echo 'abort: visual studio 2013 is not installed'
    exit -1 
} 

cmd /c "vcvarsall.bat&set" |
foreach {
  if ($_ -match "=") {
    $v = $_.split("="); set-item -force -path "ENV:\$($v[0])"  -value "$($v[1])"
  }
}
popd
write-host "`nVisual Studio 2013 Command Prompt variables set." -ForegroundColor Yellow

Function Build([string]$config, [string]$platform)
{
	echo "Starting $config $platform build"
	msbuild Paragon.sln /t:clean /p:Configuration=$config /p:Platform=$platform
	msbuild Paragon.sln /t:rebuild /p:Configuration=$config /p:Platform=$platform
	$last=$?
	if ($last -eq $False) 
	{
		write-host "failed building $config $platform"
		exit -1 
	}
	msbuild Symphony.sln /t:clean /p:Configuration=$config /p:Platform=$platform
	msbuild Symphony.sln /t:rebuild /p:Configuration=$config /p:Platform=$platform
	$last=$?
	if ($last -eq $False) 
	{
		write-host "failed building $config $platform"
		exit -1 
	}
}

# build stuff
Build "Release" "x86"

exit 1