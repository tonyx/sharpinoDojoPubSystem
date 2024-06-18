namespace PubSystem
open System
open Sharpino
open Sharpino.Core
open Sharpino.Utils
open Sharpino.Result
open Sharpino.Definitions
open PubSystem.Commons
open FSharpPlus
open MBrace.FsPickler.Json
open FsToolkit.ErrorHandling
module Kitchen =
    type Kitchen(dishRefs: List<Guid>, ingredientRefs: List<Guid> ) =

        member this.DishRefs = dishRefs
        member this.IngredientRefs = ingredientRefs
        member this.AddDishRef (dishRef: Guid) =
            result {
                do! 
                    this.DishRefs 
                    |> List.contains dishRef
                    |> not
                    |> Result.ofBool "DishRef already exists"
                return Kitchen (dishRef :: dishRefs, ingredientRefs)
            }
        member this.RemoveDishRef (dishRef: Guid) =
            result {
                do! 
                    this.DishRefs 
                    |> List.contains dishRef
                    |> Result.ofBool "DishRef does not exist"
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
        static member Deserialize  (json: Json): Result<Kitchen, string>  =
            globalSerializer.Deserialize<Kitchen> json  

        member this.Serialize  =
            this |> globalSerializer.Serialize





