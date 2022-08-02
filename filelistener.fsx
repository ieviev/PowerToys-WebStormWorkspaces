open System
open System.Diagnostics
open System.IO

#nowarn "20"

let rec copydir src dest =
    let dir = src |> DirectoryInfo
    if not dir.Exists then failwith $"directory {src} doesnt exist"
    let dirs = dir.GetDirectories()
    Directory.CreateDirectory(dest)
    for file in dir.GetFiles() do
        let target = Path.Combine(dest,file.Name)
        file.CopyTo(target)

    for subdir in dirs do
        let newdest = Path.Combine(dest,subdir.Name)
        copydir subdir.FullName newdest
    



let watcher = 
    new System.IO.FileSystemWatcher(
        ".","*.fs" ,
        EnableRaisingEvents=true ,
        IncludeSubdirectories=true
    )

let rec loop (nextproctime:DateTimeOffset) =
    let f = watcher.WaitForChanged(System.IO.WatcherChangeTypes.Changed)
    match DateTimeOffset.Now > nextproctime with 
    | false -> 
        stdout.WriteLine $"waiting for {nextproctime - DateTimeOffset.Now}"
        loop (nextproctime) 
    | true -> 
        stdout.WriteLine "STARTING FILE LISTENER EVENT"
        // kill old process
        Process.GetProcessesByName("PowerToys") |> Seq.iter (fun f -> f.Kill())
        // build project
        let buildscript = 
            new Process(StartInfo=ProcessStartInfo(
                "dotnet","publish -c Release -o publish -r win-x64 --framework net6.0-windows --no-self-contained"))
        buildscript.Start()
        buildscript.WaitForExit()
        let out = @"%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\WebStormWorkspaces\" |> Environment.ExpandEnvironmentVariables

        // delete old project
        Directory.Delete(out,true)
        // replace 
        if (not(Directory.Exists(out))) then 
            Directory.CreateDirectory(out) |> ignore

        copydir "./publish" out
        //
        let mergescript = 
            new Process(StartInfo=ProcessStartInfo(
                "ILRepack.exe",
                [
                    $"/out:WebStormWorkspaces.dll"
                    $"FSharp.Core.dll"
                    $"FSharp.Data.dll"
                    "PowerToys.Run.Plugin.WebStormWorkspaces.dll"
                ]
                |> String.concat " ",
                WorkingDirectory = out,
                UseShellExecute = false
            ))
        mergescript.Start()
        mergescript.WaitForExit()
        // let psstartinfo = ProcessStartInfo("powershell","./postbuild.ps1")
        // use proc = new Process(StartInfo=psstartinfo)
        // proc.Start() |> ignore
        // proc.WaitForExit()   
        let powertoys = 
            new Process(StartInfo=ProcessStartInfo(@"C:\Program Files\PowerToys\PowerToys.exe", "" ,UseShellExecute=true))
        powertoys.Start()
        loop (nextproctime.AddSeconds(20))   
        
loop DateTimeOffset.Now |> ignore