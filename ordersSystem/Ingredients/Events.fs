
namespace PubSystem

open System
open Sharpino
open Sharpino.Core
open Sharpino.Utils
open Sharpino.Result
open Sharpino.Definitions
open MBrace.FsPickler.Json
open PubSystem.Shared.Definitions
open FSharpPlus
open FsToolkit.ErrorHandling
open PubSystem.Ingredients

module IngredientEvents =
    type IngredientEvents =
        | IngredientTypeAdded of IngredientTypes
        | IngredientTypeRemoved of IngredientTypes
        | IngredientMeasureAdded  of IngredientMeasures
        | IngredientMeasureRemoved of IngredientMeasures
        | NameUpdated of string
            interface Event<Ingredient> with
                member this.Process (ingredient: Ingredient) =
                    match this with
                    | IngredientTypeAdded ingredientType -> ingredient.AddIngredientType ingredientType 
                    | IngredientTypeRemoved ingredientType -> ingredient.RemoveIngredientType ingredientType
                    | NameUpdated newName -> ingredient.UpdateName newName
                    | IngredientMeasureAdded ingredientMeasure -> ingredient.AddIngredientMeasure ingredientMeasure
                    | IngredientMeasureRemoved ingredientMeasure -> ingredient.RemoveIngredientMeasure ingredientMeasure
            static member Deserialize (serializer: ISerializer, json: Json): Result<IngredientEvents, string>  =
                serializer.Deserialize<IngredientEvents> json
            member this.Serialize (serializer: ISerializer) =
                this
                |> serializer.Serialize

