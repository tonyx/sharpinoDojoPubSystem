namespace PubSystem
open System
open Sharpino
open Sharpino.Core
open Sharpino.Utils
open Sharpino.Result
open Sharpino.Definitions
open FSharpPlus
open MBrace.FsPickler.Json
open FsToolkit.ErrorHandling
module Kitchen =
    type Kitchen(dishRefs: List<Guid>, ingredientRefs: List<Guid> ) =
        let stateId = Guid.NewGuid()

        member this.StateId = stateId
        member this.DishRefs = dishRefs
        member this.IngredientRefs = ingredientRefs
        member this.AddDishRef (dishRef: Guid) =
            result {
                do! 
                    this.DishRefs 
                    |> List.contains dishRef
                    |> not
                    |> Result.ofBool "DigheRef already exists"
                return Kitchen (dishRef :: dishRefs, ingredientRefs)
            }
        member this.RemoveDishRef (dishRef: Guid) =
            result {
                do! 
                    this.DishRefs 
                    |> List.contains dishRef
                    |> Result.ofBool "DigheRef does not exist"
                return Kitchen (this.DishRefs |> List.filter ((<>) dishRef), ingredientRefs)
            }
        member this.AddIngredientRef (ingredientRef: Guid) =
            result {
                do! 
                    this.IngredientRefs 
                    |> List.contains ingredientRef
                    |> not
                    |> Result.ofBool "IngredientRef already exists"
                return Kitchen (dishRefs, ingredientRef :: ingredientRefs)
            }
        member this.RemoveIngredientRef (ingredientRef: Guid) =
            result {
                do! 
                    this.IngredientRefs 
                    |> List.contains ingredientRef
                    |> Result.ofBool "IngredientRef does not exist"
                return Kitchen (dishRefs, this.IngredientRefs |> List.filter ((<>) ingredientRef))
            }

        member this.GetDishRefs() =
            this.DishRefs

        static member Zero = Kitchen([], [])

        static member StorageName =
            "_kitchen"
        static member Version =
            "_01"
        static member SnapshotsInterval =
            15
        static member Lock =
            new Object()
        static member Deserialize (serializer: ISerializer, json: Json): Result<Kitchen, string>  =
            serializer.Deserialize<Kitchen> json

        member this.Serialize (serializer: ISerializer) =
            this
            |> serializer.Serialize





