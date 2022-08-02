
try {
    dotnet publish -c Release -o publish -r win-x64 --framework net6.0-windows --no-self-contained
} 
catch {
    exit
}

$powertoys = Get-Process -n PowerToys -ErrorAction SilentlyContinue
if ($powertoys) { Stop-Process -n PowerToys }

$localpublish = "publish"
$out = "$env:LOCALAPPDATA\Microsoft\PowerToys\PowerToys Run\Plugins\WebStormWorkspaces\"
# delete old publish folder
# Remove-Item -Recurse -Force $out
# copy new publish folder
Copy-Item -Recurse $localpublish $out -Force
$currlocation = Get-Location
Set-Location $out

ILRepack.exe /out:WebStormWorkspaces.dll `
FSharp.Core.dll `
FSharp.Data.dll `
PowerToys.Run.Plugin.WebStormWorkspaces.dll 


Start-Process 'C:\Program Files\PowerToys\PowerToys.exe'

Set-Location $currlocation

