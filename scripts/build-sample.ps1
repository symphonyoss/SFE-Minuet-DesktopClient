md bin\apps

if (!(Test-Path bin\paragon\Paragon.AppPackager)) { 
    echo 'Minuet AppPackager (Paragon.AppPackager.exe) not found.  Ensure Minuet has been build: powershell ./scripts/build.ps1'
    exit -1
}; 

$build_command = 'bin\paragon\Paragon.AppPackager --i:sample-app --o:bin\apps\sample.pgx'
iex $build_command

exit 1
