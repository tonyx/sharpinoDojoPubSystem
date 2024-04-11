namespace PubSystem
open System
open PubSystem.Kitchen
open PubSystem.KitchenEvents
open Sharpino
open Sharpino.Core
open Sharpino.Utils
open Sharpino.Result
open Sharpino.Definitions
open FSharpPlus
open FsToolkit.ErrorHandling

module KitchenCommands =
    type KitchenCommands =
        | AddDishRef of Guid
        | RemoveDishRef of Guid
            interface Command<Kitchen, KitchenEvents> with
                member this.Execute (kitchen: Kitchen) =
                    match this with
                    | AddDishRef digheRef -> 
                        kitchen.AddDishRef digheRef
                        |> Result.map (fun _ -> [DishRefAdded digheRef])
                    | RemoveDishRef digheRef -> 
                        kitchen.RemoveDishRef digheRef
                        |> Result.map (fun _ -> [DishRefRemoved digheRef])
                member this.Undoer = None