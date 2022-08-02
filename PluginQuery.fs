module PowerToys.Run.Plugin.WebStormWorkspaces.PluginQuery

open System
open System.IO

let recentProjectsFile =
    @"%APPDATA%\JetBrains\"
    |> Environment.ExpandEnvironmentVariables
    |> Directory.EnumerateDirectories
    |> Seq.tryFind (fun f ->
        f |> Path.GetFileName
        |> (fun f -> f.StartsWith "WebStorm20")
    )
    |> Option.map (fun f -> Path.Combine(f, "options"))
    |> Option.map (fun f -> Path.Combine(f, "recentProjects.xml"))
    |> Option.defaultWith (fun f -> failwith @"could not find recentProjects.xml in %APPDATA%\JetBrains\WebStorm20..")

let [<Literal>] SampleProjects = __SOURCE_DIRECTORY__ + "/samples/recentProjects.xml"
type WSProjectProvider = FSharp.Data.XmlProvider<SampleProjects>


module Metadata =
    let (|ReplaceEnvVar|_|) (pattern:string) (str:string) =  
        if str.Contains(pattern)  
        then Some(fun replacement ->
            str.Replace(pattern, Environment.ExpandEnvironmentVariables(replacement)))  
        else None
    let expandEnvVar str = str |> Environment.ExpandEnvironmentVariables
    let replaceEnvVar var =
        match var with
        | ReplaceEnvVar "$USER_HOME$" fn -> fn "%USERPROFILE%"
        | containsvar when containsvar.Contains("$") -> failwithf "unknown variable in %A" containsvar
        | _ -> var
        

let projects = WSProjectProvider.Load(recentProjectsFile)

let entries = 
    projects.Component.Options
    |> Seq.tryFind (fun f -> f.Name = "additionalInfo")
    |> Option.bind (fun f -> f.Map)
    |> Option.map (fun f -> f.Entries)
    

type WebStormSolutionEntry =
    {
        title : string
        location : string
        lastOpened : DateTimeOffset
    }    


module WebStormSolutionEntry =
    let ofxmlEntry (entry:WSProjectProvider.Entry) = 
        {
            location = entry.Key |> Metadata.replaceEnvVar |> Metadata.expandEnvVar
            title = entry.Value.RecentProjectMetaInfo.FrameTitle
            lastOpened = 
                entry.Value.RecentProjectMetaInfo.Options 
                |> Seq.tryFind (fun f -> f.Name = "projectOpenTimestamp")
                |> Option.bind (fun f -> f.Value.Number)
                |> Option.map DateTimeOffset.FromUnixTimeMilliseconds
                |> Option.defaultValue DateTimeOffset.Now
        }

let getWSProjects() = 
    match entries with 
    | None -> failwith @"could not find recentProjects.xml in %APPDATA%\JetBrains\WebStorm20.."
    | Some ent -> ent |> Array.map WebStormSolutionEntry.ofxmlEntry    