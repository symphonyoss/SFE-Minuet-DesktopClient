echo "Set Visual Studio env vars"

$vcPaths='c:\Program Files (x86)\Microsoft Visual Studio 12.0\VC',
       'c:\Program Files\Microsoft Visual Studio 12.0\VC',
       'c:\Program Files (x86)\Microsoft Visual Studio 14.0\VC',
       'c:\Program Files\Microsoft Visual Studio 14.0\VC',
       'c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools',
       'c:\Program Files\Microsoft Visual Studio\2017\Community\Common7\Tools'

foreach ($vcPath in $vcPaths) {
    if ((Test-Path $vcPath) -eq $True) {
        $vsFound = $True
        pushd $vcPath
        break
    }
}

if (!$vsFound) {
    echo 'abort: Visual Studio is not installed'
    exit -1 
}

foreach ($bat in 'vcvarall.bat', 'vsdevcmd.bat') {
    if ((Test-Path (Join-Path $vcPath $bat)) -eq $True) {
        $vcBatFile = $bat
    }
}

if (!$vcBatFile) {
    echo 'abort: Visual Studio environment bat file is not installed'
    exit -1 
}

cmd /c $vcBatFile + "&set" |
foreach {
  if ($_ -match "=") {
    $v = $_.split("="); set-item -force -path "ENV:\$($v[0])"  -value "$($v[1])"
  }
}
popd
write-host "`nVisual Studio Command Prompt variables set." -ForegroundColor Yellow

Function Build([string]$config, [string]$platform)
{
	echo "Starting $config $platform build"
	msbuild Minuet.sln /t:clean /p:Configuration=$config /p:Platform=$platform
	msbuild Minuet.sln /t:rebuild /p:Configuration=$config /p:Platform=$platform
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
