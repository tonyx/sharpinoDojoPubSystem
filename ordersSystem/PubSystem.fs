
namespace PubSystem
open System
open PubSystem.Kitchen
open PubSystem.Dishes
open PubSystem.KitchenEvents
open PubSystem.KitchenCommands
open PubSystem.DishEvents
open PubSystem.DishCommands
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
open PubSystem.Shared.Definitions
open Sharpino.Storage

module PubSystem =
    open Ingredients
    let doNothingBroker: IEventBroker<_> =
        {
            notify = None
            notifyAggregate = None
        }


    type PubSystem(eventStore: IEventStore<string>, eventBroker: IEventBroker<string>) =
        let kitchenStateViewer = getStorageFreshStateViewer<Kitchen, KitchenEvents, string> eventStore 
        let dishStateViewer = getAggregateStorageFreshStateViewer<Dish, DishEvents, string> eventStore
        let ingredientStateViewer = getAggregateStorageFreshStateViewer<Ingredient, IngredientEvents, string> eventStore

        new (eventStore: IEventStore<string>) =
            new PubSystem(eventStore, doNothingBroker)

        member this.GetDishReferences() =   
            result {
                let! (_, state) = kitchenStateViewer()
                return state.DishRefs
            }
        member this.AddDish (dish: Dish) =
            result {
                return! 
                    dish.Id
                    |> AddDishRef
                    |> runInitAndCommand<Kitchen, KitchenEvents, Dish, string> eventStore eventBroker dish
            }

        member this.RemoveDish (dishId: Guid) =
            result {
                let deactivateDish = 
                    DishCommands.Deactivate
                let! deactivated =
                    deactivateDish
                    |> runAggregateCommand<Dish, DishEvents, string> dishId eventStore eventBroker 
                let result =
                    dishId
                    |> RemoveDishRef
                    |> runCommand<Kitchen, KitchenEvents, string> eventStore eventBroker 

                return! result
            }


        member this.AddIngredient (ingredient: Ingredient) =
            result {
                return! 
                    ingredient.Id
                    |> AddIngredientRef
                    |> runInitAndCommand<Kitchen, KitchenEvents, Ingredient, string> eventStore eventBroker ingredient
            }
        member this.AddIngredientToDish (dishId: Guid, ingredientId: Guid) =
            result {
                // you may want to use lock here to prevent ingredient from being deleted while adding it to a dish
                let! ingredientExists = this.GetIngredient ingredientId 

                // you may want to use the alternative approach by getting the ingredient using the ingredientStateViewer
                // let! ingredientExists= ingredientStateViewer ingredientId
                // but that would fail in being able to retrieve an ingredient that existed and was deleted from the kitchen
                return! 
                    ingredientId
                    |> AddIngredient
                    |> runAggregateCommand<Dish, DishEvents, string> dishId eventStore eventBroker
            }

        member this.GetIngredients () =
            result {
                let! (_, state) = kitchenStateViewer()
                return state.IngredientRefs
            }

        member this.GetIngredient (ingredientId: Guid) =
            result {
                let! (_, state) = ingredientStateViewer ingredientId
                let! active = 
                    state.Active 
                    |> Result.ofBool "Ingredient was deleted"
                return state
            }

        member this.UpdateDishName (dishId: Guid, newName: string) =
            result {
                return! 
                    newName
                    |> UpdateName
                    |> runAggregateCommand<Dish, DishEvents, string> dishId eventStore eventBroker 
            }
        
        member this.GetAllIngredientRefs () =
            result {
                let! (_, state) = kitchenStateViewer()
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
                    |> runAggregateCommand<Ingredient, IngredientEvents, string> ingredientId eventStore eventBroker
            }

        member this.GetDish (dishId: Guid) =
            result {
                let! (_, state) = dishStateViewer dishId
                let! active = 
                    state.Active 
                    |> Result.ofBool "Dish was deleted"
                return state
            }

        member this.GetDishRefs () =
            result {
                let! (_, state) = kitchenStateViewer()
                return state.GetDishRefs()
            }

        member this.AddTypesToDish (dishId: Guid, dishTypes: DishTypes) =
            result {
                return! 
                    dishTypes
                    |> AddDishType
                    |> runAggregateCommand<Dish, DishEvents, string> dishId eventStore eventBroker
            }

        member this.RemoveTypeFromDish (dishId: Guid, dishTypes: DishTypes) =
            result {
                return! 
                    dishTypes
                    |> RemoveDishType
                    |> runAggregateCommand<Dish, DishEvents, string> dishId eventStore eventBroker
            }
        member this.RemoveTypeFromIngredient (ingredientId: Guid, ingredientType: IngredientTypes) =
            result {
                return! 
                    ingredientType
                    |> IngredientCommands.RemoveIngredientType
                    |> runAggregateCommand<Ingredient, IngredientEvents, string> ingredientId eventStore eventBroker
            }

        member this.GetDishes () =
            result {
                let! dishRefs = this.GetDishRefs()
                let! dishes = dishRefs |> List.map this.GetDish |> Result.sequence
                return dishes |> Array.toList 
            }

