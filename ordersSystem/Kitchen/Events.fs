
namespace PubSystem
open System
open PubSystem.Kitchen
open PubSystem.Commons
open Sharpino

open Sharpino.Core
open Sharpino.Utils
open Sharpino.Result
open Sharpino.Definitions
open FSharpPlus
open FsToolkit.ErrorHandling
module KitchenEvents =
    type KitchenEvents =
        | DishRefAdded of Guid
        | DishRefRemoved of Guid
        | IngredientRefAdded of Guid
        | IngredientRefRemoved of Guid

            interface Event<Kitchen> with
                member this.Process (kitchen: Kitchen) =
                    match this with
                    | DishRefAdded digheRef -> 
                        kitchen.AddDishRef digheRef
                    | DishRefRemoved digheRef -> 
                        kitchen.RemoveDishRef digheRef
                    | IngredientRefAdded ingredientRef ->
                        kitchen.AddIngredientRef ingredientRef
                    | IngredientRefRemoved ingredientRef ->
                        kitchen.RemoveIngredientRef ingredientRef
            static member Deserialize (json: Json): Result<KitchenEvents, string>  =
                globalSerializer.Deserialize<KitchenEvents> json
            member this.Serialize  =
                this
                |> globalSerializer.Serialize