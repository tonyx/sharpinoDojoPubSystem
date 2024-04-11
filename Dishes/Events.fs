namespace PubSystem

open System
open Sharpino
open PubSystem.Kitchen
open Sharpino.Core
open Sharpino.Utils
open PubSystem.Dishes
open Sharpino.Result
open Sharpino.Definitions
open FSharpPlus
open FsToolkit.ErrorHandling

module DishEvents =
    type DishEvents =
        | AddDishType of DishTypes
            interface Event<Dish> with
                member this.Process (dish: Dish) =
                    match this with
                    | AddDishType dishType -> dish.AddDishType dishType
            static member Deserialize (serializer: ISerializer, json: Json): Result<DishEvents, string>  =
                serializer.Deserialize<DishEvents> json
            member this.Serialize (serializer: ISerializer) =
                this
                |> serializer.Serialize