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
    type Kitchen(digheRefs: List<Guid>) =
        let stateId = Guid.NewGuid()

        member this.StateId = stateId
        member this.DigheRefs = digheRefs
        member this.AddDishRef (digheRef: Guid) =
            result {
                do! 
                    this.DigheRefs 
                    |> List.contains digheRef
                    |> not
                    |> Result.ofBool "DigheRef already exists"
                return Kitchen (digheRef :: digheRefs)
            }
        member this.RemoveDishRef (digheRef: Guid) =
            result {
                do! 
                    this.DigheRefs 
                    |> List.contains digheRef
                    |> Result.ofBool "DigheRef does not exist"
                return Kitchen (this.DigheRefs |> List.filter ((<>) digheRef))
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





