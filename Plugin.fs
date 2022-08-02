namespace PowerToys.Run.Plugin.WebStormWorkspaces

open System
open System.Diagnostics
open System.IO
open PowerToys.Run.Plugin.WebStormWorkspaces.PluginQuery
open Wox.Plugin

type WebStormPlugin() =
    let data = lazy getWSProjects()
    let createResult name sub (act:ActionContext->bool) =
        Result(
            Action=Func<ActionContext,bool>act,
            SubTitle=sub,
            Title=name
        )
    
        
    interface IPlugin with
        member this.Description: string = "WebStormWorkspaces"
        member this.Name: string = "WebStormWorkspaces"
        member this.Init(context) = data.Force() |> ignore
        member this.Query(query) =
            data.Value
            |> Seq.choose (fun f ->
                let slnname = f.location |> Path.GetFileNameWithoutExtension
                
                let mutable score =
                    if slnname.Equals(query.Search,StringComparison.OrdinalIgnoreCase) then Int32.MaxValue
                    elif slnname.StartsWith(query.Search,StringComparison.OrdinalIgnoreCase) then (Int32.MaxValue / 2)
                    elif slnname.ToLower().Contains(query.Search.ToLower()) then (Int32.MaxValue / 4)
                    else 0
                if score = 0 then None else
                let dayspassed = DateTimeOffset.Now.Subtract(f.lastOpened).TotalHours |> int    
                score <- score - dayspassed
                
                let folder = f.location |> Path.GetDirectoryName
                let dateonly = f.lastOpened.ToString("yyyy-MM-dd")
                let subtitle = $"Project Folder: {folder}, Last opened {dateonly} "
                let res =
                    createResult slnname subtitle (fun d ->
                        ProcessStartInfo(f.location,"",UseShellExecute=true)
                        |> Process.Start
                        |> ignore
                        true
                    )
                res.Score <- score
                res.IcoPath <- "images\\webstorm.png"
                if res.Score = -1 then None else Some res
            )
            |> ResizeArray

            
                

                
                
                
                
