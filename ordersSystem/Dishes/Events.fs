namespace PubSystem

open System
open Sharpino
open PubSystem.Kitchen
open Sharpino.Core
open Sharpino.Utils
open PubSystem.Dishes
open Sharpino.Result
open Sharpino.Definitions
open MBrace.FsPickler.Json
open FSharpPlus
open FsToolkit.ErrorHandling

module DishEvents =
    type DishEvents =
        | DishTypeAdded of DishTypes
        | DishTypeRemoved of DishTypes
        | NameUpdated of string
            interface Event<Dish> with
                member this.Process (dish: Dish) =
                    match this with
                    | DishTypeAdded dishType -> dish.AddDishType dishType
                    | DishTypeRemoved dishType -> dish.RemoveDishType dishType
                    | NameUpdated newName -> dish.UpdateName newName
            static member Deserialize (serializer: ISerializer, json: Json): Result<DishEvents, string>  =
                serializer.Deserialize<DishEvents> json
            member this.Serialize (serializer: ISerializer) =
                this
                |> serializer.Serialize