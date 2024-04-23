
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
open PubSystem.IngredientEvents
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
    open Ingredients
    let doNothingBroker: IEventBroker =
        {
            notify = None
            notifyAggregate = None
        }

    type PubSystem(eventStore: IEventStore) =
        let kitchenStateViewer = getStorageFreshStateViewer<Kitchen, KitchenEvents> eventStore 
        let dishStateViewer = getAggregateStorageFreshStateViewer<Dish, DishEvents> eventStore

        let ingredientStateViewer = getAggregateStorageFreshStateViewer<Ingredient, IngredientEvents> eventStore
        member this.GetDishReferences() =   
            result {
                let! (_, state,_, _) = kitchenStateViewer()
                return state.DishRefs
            }
        member this.AddDish (dish: Dish) =
            result {
                return! 
                    dish.Id
                    |> AddDishRef
                    |> runInitAndCommand<Kitchen, KitchenEvents, Dish> eventStore doNothingBroker kitchenStateViewer dish
            }

        member this.AddIngredient (ingredient: Ingredient) =
            result {
                return! 
                    ingredient.Id
                    |> AddIngredientRef
                    |> runInitAndCommand<Kitchen, KitchenEvents, Ingredient> eventStore doNothingBroker kitchenStateViewer ingredient
            }
        member this.GetIngredients () =
            result {
                let! (_, state,_, _) = kitchenStateViewer()
                return state.IngredientRefs
            }

        member this.GetIngredient (ingredientId: Guid) =
            result {
                let! (_, state,_, _) = ingredientStateViewer ingredientId
                return state
            }

        member this.UpdateDishName (dishId: Guid, newName: string) =
            result {
                return! 
                    newName
                    |> UpdateName
                    |> runAggregateCommand<Dish, DishEvents> dishId eventStore doNothingBroker dishStateViewer
            }
        
        member this.GetAllIngredientRefs () =
            result {
                let! (_, state,_, _) = kitchenStateViewer()
                return state.IngredientRefs
            }
        member this.GetAllIngredients () =
            result {
                let! ingredientRefs = this.GetIngredients()
                let! ingredients = ingredientRefs |> List.map this.GetIngredient |> Result.sequence
                return ingredients |> Array.toList 
            }

        member this.AddTypeToIngredient (ingredientId: Guid, ingredientType: IngredientTypes) =
            result {
                return! 
                    ingredientType
                    |> IngredientCommands.AddIngredientType
                    |> runAggregateCommand<Ingredient, IngredientEvents> ingredientId eventStore doNothingBroker ingredientStateViewer
            }

        member this.GetDish (dishId: Guid) =
            result {
                let! (_, state,_, _) = dishStateViewer dishId
                return state
            }

        member this.GetDishRefs () =
            result {
                let! (_, state,_ , _) = kitchenStateViewer()
                return state.GetDishRefs()
            }

        member this.AddTypesToDish (dishId: Guid, dishTypes: DishTypes) =
            result {
                return! 
                    dishTypes
                    |> AddDishType
                    |> runAggregateCommand<Dish, DishEvents> dishId eventStore doNothingBroker dishStateViewer
            }

        member this.RemoveTypeFromDish (dishId: Guid, dishTypes: DishTypes) =
            result {
                return! 
                    dishTypes
                    |> RemoveDishType
                    |> runAggregateCommand<Dish, DishEvents> dishId eventStore doNothingBroker dishStateViewer
            }
        member this.RemoveTypeFromIngredient (ingredientId: Guid, ingredientType: IngredientTypes) =
            result {
                return! 
                    ingredientType
                    |> IngredientCommands.RemoveIngredientType
                    |> runAggregateCommand<Ingredient, IngredientEvents> ingredientId eventStore doNothingBroker ingredientStateViewer
            }

        member this.GetDishes () =
            result {
                let! dishRefs = this.GetDishRefs()
                let! dishes = dishRefs |> List.map this.GetDish |> Result.sequence
                return dishes |> Array.toList 
            }

