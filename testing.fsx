
// let rnduuid = System.Guid.NewGuid().ToString().Replace("-","")

//exports from: ./lib
#r @"lib\Wox.Infrastructure.dll"
#r @"lib\Wox.Plugin.dll"
//
#r @"bin\Debug\net6.0-windows\PowerToys.Run.Plugin.WebStormWorkspaces.dll"

#r "nuget: FSharp.Data"

open PowerToys.Run.Plugin.WebStormWorkspaces
open PowerToys.Run.Plugin.WebStormWorkspaces.PluginQuery
open System

let rec1 = recentProjectsFile.Value

let [<Literal>] SampleProjects = __SOURCE_DIRECTORY__ + "/samples/recentProjects.xml"
type WSProjectProvider = FSharp.Data.XmlProvider<SampleProjects>


let projects = WSProjectProvider.Load(SampleProjects)

let entries = 
    projects.Component.Options
    |> Seq.tryFind (fun f -> f.Name = "additionalInfo")
    |> Option.bind (fun f -> f.Map)
    |> Option.map (fun f -> f.Entries)
    

let entriesformatted = 
    entries.Value
    |> Array.map (fun f -> 
        {|
            location = f.Key
            title = f.Value.RecentProjectMetaInfo.FrameTitle
            lastOpened = 
                f.Value.RecentProjectMetaInfo.Options 
                |> Seq.tryFind (fun f -> f.Name = "projectOpenTimestamp")
                |> Option.bind (fun f -> f.Value.Number)
                |> Option.map DateTimeOffset.FromUnixTimeMilliseconds
                |> Option.defaultValue DateTimeOffset.Now
        |}
    )


//let recentSolutions = 
//    PluginQuery.recentSolutions

//let xmlentries = 
//    recentSolutions 
//    |> Seq.map SolutionInfoRecord.ofXmlEntry 
//    |> Seq.toArray


//do xmlentries |> Seq.iter (printfn "%A")
