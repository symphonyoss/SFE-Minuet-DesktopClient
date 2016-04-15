echo "clean output dirs"
if (Test-Path bin) {
    rd bin -recurse   
}

echo "clean bin and obj outputs"
if (Test-Path symphony\Symphony\bin) {
    rd symphony\Symphony\bin -recurse
}
if (Test-Path symphony\Symphony\obj) {
    rd symphony\Symphony\obj -recurse
}
if (Test-Path symphony\Symphony.Crypto\bin) {
    rd symphony\Symphony.Crypto\bin -recurse
}
if (Test-Path symphony\Symphony.Crypto\obj) {
    rd symphony\Symphony.Crypto\obj -recurse
}