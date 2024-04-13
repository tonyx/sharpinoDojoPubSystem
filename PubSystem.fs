
namespace PubSystem
open System
open PubSystem.Kitchen
open PubSystem.Dishes
open PubSystem.KitchenEvents
open PubSystem.KitchenCommands
open PubSystem.DishEvents
open PubSystem.DishCommands
open PubSystem.KitchenEvents
open PubSystem.DishEvents
open Sharpino
open Sharpino.Core
open Sharpino.Utils
open Sharpino.Result
open Sharpino.PgStorage
open Sharpino.CommandHandler
open Sharpino.Definitions
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.MemoryStorage
open Sharpino.Storage

module PubSystem =
    let doNothingBroker: IEventBroker =
        {
            notify = None
            notifyAggregate = None
        }

    type PubSystem(storage: IEventStore) =
        let kitchenStateViewer = getStorageFreshStateViewer<Kitchen, KitchenEvents> storage 
        let dishStateViewer = getAggregateStorageFreshStateViewer<Dish, DishEvents> storage
        member this.GetDishReferences() =   
            result {
                let! (_, state,_, _) = kitchenStateViewer()
                return state.DishRefs
            }
        member this.AddDish (dish: Dish) =
            result {
                return! 
                    (AddDishRef dish.Id)
                    |> runInitAndCommand<Kitchen, KitchenEvents, Dish> storage doNothingBroker kitchenStateViewer dish
            }
        member this.UpdateDishName (dishId: Guid, newName: string) =
            result {
                return! 
                    (UpdateName newName)
                    |> runAggregateCommand<Dish, DishEvents> dishId storage doNothingBroker dishStateViewer
            }

        member this.GetDish (dishId: Guid) =
            result {
                let! (_, state,_, _) = dishStateViewer dishId
                return state
            }
