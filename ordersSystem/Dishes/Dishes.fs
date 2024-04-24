
namespace PubSystem
open System
open Sharpino
open Sharpino.Core
open Sharpino.Lib.Core.Commons
open Sharpino.Utils
open Sharpino.Result
open Sharpino.Definitions
open PubSystem.Shared.Definitions
open FSharpPlus
open MBrace.FsPickler.Json
open FsToolkit.ErrorHandling
open MBrace.FsPickler.Combinators

module Dishes =

    type Dish (id: Guid, name: string, dishTypes: List<DishTypes>) =

        let stateId = Guid.NewGuid()
        member this.Id = id
        member this.Name = name
        member this.DishTypes = dishTypes

        member this.AddDishType (dishType: DishTypes) =
            result {
                do! 
                    this.DishTypes 
                    |> List.contains dishType
                    |> not
                    |> Result.ofBool "DishType already exists"
                return Dish (id, name, dishType :: dishTypes)
            }

        member this.RemoveDishType (dishType: DishTypes) =
            result {
                do! 
                    this.DishTypes 
                    |> List.contains dishType
                    |> Result.ofBool "DishType does not exist"
                do! 
                    this.DishTypes 
                    |> List.length > 1
                    |> Result.ofBool "Dish must have at least one type"
                return Dish (id, name, dishTypes |> List.filter ((<>) dishType))
            }

        member this.UpdateName (newName: string) =
            result {
                do! 
                    newName
                    |> String.IsNullOrWhiteSpace
                    |> not
                    |> Result.ofBool "Name cannot be empty"
                return Dish (id, newName, dishTypes)
            }
        member this.ToDishTO  =
            { Id = id; Name = name; DishTypes = dishTypes }

        static member FromDishTO (dishTO: DishTO) =
            Dish (dishTO.Id, dishTO.Name, dishTO.DishTypes)

        member this.Serialize (serializer: ISerializer) =
            this
            |> serializer.Serialize

        static member SnapshotsInterval =
            15
        static member StorageName =
            "_dishes"
        static member Version =
            "_01"

        static member Deserialize (serializer: ISerializer, json: Json): Result<Dish, string>  =
            serializer.Deserialize<Dish> json

        interface Aggregate with
            member this.Id = id
            member this.Serialize (serializer: ISerializer) =
                this.Serialize serializer
            member this.Lock = this
            member this.StateId = stateId 
        
        interface Entity with
            member this.Id = this.Id 
        


