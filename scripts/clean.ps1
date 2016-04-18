echo "clean output dirs"
if (Test-Path bin) {
    rd bin -recurse   
}