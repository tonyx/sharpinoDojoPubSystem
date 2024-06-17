namespace PubSystem

open System
open Sharpino
open PubSystem.Kitchen
open Sharpino.Core
open Sharpino.Utils
open PubSystem.Dishes
open PubSystem.Commons
open Sharpino.Result
open Sharpino.Definitions
open PubSystem.Shared.Definitions
open MBrace.FsPickler.Json
open FSharpPlus
open FsToolkit.ErrorHandling

module DishEvents =
    type DishEvents =
        | DishTypeAdded of DishTypes
        | DishTypeRemoved of DishTypes
        | NameUpdated of string
        | IngredientAdded of Guid
        | IngredientRemoved of Guid
        | Deactivated
            interface Event<Dish> with
                member this.Process (dish: Dish) =
                    match this with
                    | DishTypeAdded dishType -> dish.AddDishType dishType
                    | DishTypeRemoved dishType -> dish.RemoveDishType dishType
                    | NameUpdated newName -> dish.UpdateName newName
                    | IngredientAdded ingredientId -> dish.AddIngredient ingredientId
                    | IngredientRemoved ingredientId -> dish.RemoveIngredient ingredientId
                    | Deactivated -> dish.Deactivate ()
            static member Deserialize  (json: Json): Result<DishEvents, string>  =
                globalSerializer.Deserialize<DishEvents> json
            member this.Serialize  =
                this
                |> globalSerializer.Serialize