if (!(Test-Path bin\paragon\Paragon.AppPackager.exe)) { 
    write-host 'Minuet AppPackager (Paragon.AppPackager.exe) not found.  Ensure Minuet has been built `powershell ./scripts/build.ps1`' -ForegroundColor Red
    exit -1
};

if (Test-Path bin\apps) {
    rd bin\apps -recurse 
}

md bin\apps 

$build_command = 'bin\paragon\Paragon.AppPackager --i:sample-app --o:bin\apps\sample.pgx'
iex $build_command

exit 1
