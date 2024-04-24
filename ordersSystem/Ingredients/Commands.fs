namespace PubSystem

open System
open Sharpino
open Sharpino.Core
open Sharpino.Utils
open Sharpino.Result
open Sharpino.Definitions
open FSharpPlus
open FsToolkit.ErrorHandling
open PubSystem.Ingredients
open PubSystem.IngredientEvents
open PubSystem.Shared.Definitions

module IngredientCommands =
    type IngredientCommands =
        | AddIngredientType of IngredientTypes
        | RemoveIngredientType of IngredientTypes
        | UpdateName of string
        | AddIngredientMeasure of IngredientMeasures
        | RemoveIngredientMeasure of IngredientMeasures
            interface Command<Ingredient, IngredientEvents> with
                member this.Execute (ingredient: Ingredient) =
                    match this with
                    | AddIngredientType ingredientType -> 
                        ingredient.AddIngredientType ingredientType
                        |> Result.map (fun _ -> [IngredientTypeAdded ingredientType])
                    | RemoveIngredientType ingredientType ->
                        ingredient.RemoveIngredientType ingredientType
                        |> Result.map (fun _ -> [IngredientTypeRemoved ingredientType])
                    | UpdateName newName ->
                        ingredient.UpdateName newName
                        |> Result.map (fun _ -> [NameUpdated newName])
                    | AddIngredientMeasure ingredientMeasure ->
                        ingredient.AddIngredientMeasure ingredientMeasure
                        |> Result.map (fun _ -> [IngredientMeasureAdded ingredientMeasure])
                    | RemoveIngredientMeasure ingredientMeasure ->
                        ingredient.RemoveIngredientMeasure ingredientMeasure
                        |> Result.map (fun _ -> [IngredientMeasureRemoved ingredientMeasure])
                member this.Undoer = None