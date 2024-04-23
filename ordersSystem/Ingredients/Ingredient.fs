namespace PubSystem

open System
open Sharpino
open Sharpino.Core
open Sharpino.Lib.Core.Commons
open Sharpino.Utils
open Sharpino.Result
open Sharpino.Definitions
open FSharpPlus
open MBrace.FsPickler.Json
open FsToolkit.ErrorHandling
open MBrace.FsPickler.Combinators

module Ingredients =
    type IngredientMeasures =
        | Grams
        | Kilograms
        | Liters
        | Milliliters
        | Pieces
        | Other of string

    type IngredientTypes =
        | Meat
        | Fish
        | Vegetable
        | Fruit
        | Dairy
        | Grain
        | Other of string

    type Ingredient (id: Guid, name: string, ingredientTypes: List<IngredientTypes>, ingredientMeasures: List<IngredientMeasures>) =

        let stateId = Guid.NewGuid()
        member this.Id = id
        member this.Name = name
        member this.IngredientTypes = ingredientTypes
        member this.IngredientMeasures = ingredientMeasures

        member this.AddIngredientType (ingredientType: IngredientTypes) =
            result {
                do! 
                    this.IngredientTypes 
                    |> List.contains ingredientType
                    |> not
                    |> Result.ofBool "IngredientType already exists"
                return Ingredient (id, name, ingredientType :: ingredientTypes, ingredientMeasures)
            }

        member this.RemoveIngredientType (ingredientType: IngredientTypes) =
            result {
                do! 
                    this.IngredientTypes 
                    |> List.contains ingredientType
                    |> Result.ofBool "IngredientType does not exist"
                return Ingredient (id, name, ingredientTypes |> List.filter ((<>) ingredientType), ingredientMeasures)
            }

        member this.UpdateName (newName: string) =
            result {
                do! 
                    newName
                    |> String.IsNullOrWhiteSpace
                    |> not
                    |> Result.ofBool "Name cannot be empty"
                return Ingredient (id, newName, ingredientTypes, ingredientMeasures)
            }

        member this.AddIngredientMeasure (ingredientMeasure: IngredientMeasures) =
            result {
                do! 
                    this.IngredientMeasures 
                    |> List.contains ingredientMeasure
                    |> not
                    |> Result.ofBool "IngredientMeasure already exists"
                return Ingredient (id, name, ingredientTypes, ingredientMeasure :: ingredientMeasures)
            }

        member this.RemoveIngredientMeasure (ingredientMeasure: IngredientMeasures) =
            result {
                do! 
                    this.IngredientMeasures 
                    |> List.contains ingredientMeasure
                    |> Result.ofBool "IngredientMeasure does not exist"
                return Ingredient (id, name, ingredientTypes, ingredientMeasures |> List.filter ((<>) ingredientMeasure))
            }

        static member SnapshotsInterval =
            15
        static member StorageName =
            "_ingredients"
        static member Version =
            "_01"

        member this.Serialize (serializer: ISerializer) =
            this
            |> serializer.Serialize

        static member Deserialize (serializer: ISerializer, json: Json): Result<Ingredient, string>  =
            serializer.Deserialize<Ingredient> json

        interface Aggregate with
            member this.Id = id
            member this.Serialize (serializer: ISerializer)= 
                this.Serialize serializer

            member this.Lock = this
            member this.StateId = stateId

        interface Entity with
            member this.Id = this.Id