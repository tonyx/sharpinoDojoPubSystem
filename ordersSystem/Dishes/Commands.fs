
namespace PubSystem

open System
open Sharpino
open PubSystem.Kitchen
open PubSystem.KitchenEvents
open Sharpino.Core
open Sharpino.Utils
open PubSystem.Dishes
open Sharpino.Result
open Sharpino.Definitions
open PubSystem.Shared.Definitions
open FSharpPlus
open FsToolkit.ErrorHandling

module DishCommands =
    open DishEvents
    type DishCommands =
        | AddDishType of DishTypes
        | RemoveDishType of DishTypes
        | UpdateName of string
        | AddIngredient of Guid
        | RemoveIngredient of Guid
        | Deactivate
            interface Command<Dish, DishEvents> with
                member this.Execute (dish: Dish) =
                    match this with
                    | AddDishType dishType -> 
                        dish.AddDishType dishType
                        |> Result.map (fun _ -> [DishTypeAdded dishType])
                    | RemoveDishType dishType ->
                        dish.RemoveDishType dishType
                        |> Result.map (fun _ -> [DishTypeRemoved dishType])
                    | UpdateName newName ->
                        dish.UpdateName newName
                        |> Result.map (fun _ -> [NameUpdated newName])
                    | AddIngredient ingredientId ->
                        dish.AddIngredient ingredientId
                        |> Result.map (fun _ -> [IngredientAdded ingredientId])
                    | RemoveIngredient ingredientId ->
                        dish.RemoveIngredient ingredientId
                        |> Result.map (fun _ -> [IngredientRemoved ingredientId])
                    | Deactivate ->
                        dish.Deactivate ()
                        |> Result.map (fun _ -> [Deactivated])
                member this.Undoer = None
