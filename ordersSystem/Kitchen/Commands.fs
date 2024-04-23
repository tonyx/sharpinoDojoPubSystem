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
        | AddIngredientRef of Guid
        | RemoveIngredientRef of Guid
            interface Command<Kitchen, KitchenEvents> with
                member this.Execute (kitchen: Kitchen) =
                    match this with
                    | AddDishRef dishRef -> 
                        kitchen.AddDishRef dishRef
                        |> Result.map (fun _ -> [DishRefAdded dishRef])
                    | RemoveDishRef digheRef -> 
                        kitchen.RemoveDishRef digheRef
                        |> Result.map (fun _ -> [DishRefRemoved digheRef])
                    | AddIngredientRef ingredientRef ->
                        kitchen.AddIngredientRef ingredientRef
                        |> Result.map (fun _ -> [IngredientRefAdded ingredientRef])
                    | RemoveIngredientRef ingredientRef ->
                        kitchen.RemoveIngredientRef ingredientRef
                        |> Result.map (fun _ -> [IngredientRefRemoved ingredientRef])

                member this.Undoer = None