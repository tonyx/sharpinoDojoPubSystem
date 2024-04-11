
namespace PubSystem
open System
open PubSystem.Kitchen
open Sharpino
open Sharpino.Core
open Sharpino.Utils
open Sharpino.Result
open Sharpino.Definitions
open FSharpPlus
open FsToolkit.ErrorHandling
module KitchenEvents =
    type KitchenEvents =
        | DishRefAdded of Guid
        | DishRefRemoved of Guid

            interface Event<Kitchen> with
                member this.Process (kitchen: Kitchen) =
                    match this with
                    | DishRefAdded digheRef -> 
                        kitchen.AddDishRef digheRef
                    | DishRefRemoved digheRef -> 
                        kitchen.RemoveDishRef digheRef
            static member Deserialize (serializer: ISerializer, json: Json): Result<KitchenEvents, string>  =
                serializer.Deserialize<KitchenEvents> json
            member this.Serialize (serializer: ISerializer) =
                this
                |> serializer.Serialize