#Requires -Modules @{ ModuleName="Microsoft.PowerShell.Archive"; ModuleVersion="1.2.3" }
$MyInvocation.MyCommand.Path | Split-Path | Push-Location # Run from this script's directory
$Name = ((ls . -filter *.csproj -recurse | sort).BaseName) | select -last 1
dotnet build -c Release
curl -L https://github.com/ToniMacaroni/ComputerInterface/releases/download/1.4.12/ComputerInterface.zip -o DL.zip
Expand-Archive DL.zip 
rm DL.zip
mv DL\BepInEx .
rm DL
cp .\ReleaseZip\BepInEx\plugins\$Name\$Name* .\BepInEx\plugins\$Name\
Compress-Archive .\BepInEx\ $Name-v
rmdir .\BepInEx\ -R
rm ReleaseZip -R
Pop-Location
