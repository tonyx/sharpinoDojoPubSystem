
namespace PubSystem
open System
open Sharpino
open Sharpino.Core
open Sharpino.Utils
open Sharpino.Result
open Sharpino.Definitions
open FSharpPlus
open FsToolkit.ErrorHandling
module Dishes =
    open Sharpino.Lib.Core.Commons
    type DishTypes =
        | Starter
        | Main
        | Dessert
        | Drink
        | Other of string

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
                return Dish (id, name, dishTypes |> List.filter ((<>) dishType))
            }

        member this.UpdateName (newName: string) =
            result {
                do! 
                    newName
                    |> String.IsNullOrWhiteSpace
                    |> Result.ofBool "Name cannot be empty"
                return Dish (id, newName, dishTypes)
            }

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
        


