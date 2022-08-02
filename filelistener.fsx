open System
open System.Diagnostics

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
        let psstartinfo = ProcessStartInfo("powershell","./postbuild.ps1")
        use proc = new Process(StartInfo=psstartinfo)
        proc.Start() |> ignore
        proc.WaitForExit()   
        loop (nextproctime.AddSeconds(20))   
        
loop DateTimeOffset.Now |> ignore