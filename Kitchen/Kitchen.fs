namespace PubSystem
open System
open Sharpino
open Sharpino.Core
open Sharpino.Utils
open Sharpino.Result
open Sharpino.Definitions
open FSharpPlus
open FsToolkit.ErrorHandling
module Kitchen =
    type Kitchen(dishRefs: List<Guid>) =
        let stateId = Guid.NewGuid()

        member this.StateId = stateId
        member this.DishRefs = dishRefs
        member this.AddDishRef (digheRef: Guid) =
            result {
                do! 
                    this.DishRefs 
                    |> List.contains digheRef
                    |> not
                    |> Result.ofBool "DigheRef already exists"
                return Kitchen (digheRef :: dishRefs)
            }
        member this.RemoveDishRef (dishRef: Guid) =
            result {
                do! 
                    this.DishRefs 
                    |> List.contains dishRef
                    |> Result.ofBool "DigheRef does not exist"
                return Kitchen (this.DishRefs |> List.filter ((<>) dishRef))
            }

        static member Zero = Kitchen([])

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





