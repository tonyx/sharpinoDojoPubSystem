namespace PubSystem.Shared

open System

module Definitions =
    type IngredientMeasures =
        | Grams
        | Kilograms
        | Liters
        | Milliliters
        | Pieces
        | Other of string

    type IngredientTypes =
        | Meat
        | Fish
        | Vegetable
        | Fruit
        | Dairy
        | Grain
        | Other of string

    type DishTypes =
        | Starter
        | Main
        | Dessert
        | Drink
        | Other of string

    type IngredientTO =
        {
            Id: Guid
            Name: string
            IngredientTypes: List<IngredientTypes>
            IngredientMeasures: List<IngredientMeasures>
        }

    type DishTO =
        {
            Id: Guid
            Name: string
            DishTypes: List<DishTypes>
        }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName


module Services =
    open Definitions
    type IRestPubSystem =
        {
            AddDish: DishTO -> Async<Result<unit, string>>
            AddIngredient: IngredientTO -> Async<Result<unit, string>>
            GetIngredients: unit -> Async<Result<List<IngredientTO>, string>>
            UpdateDishName: Guid * string -> Async<Result<unit, string>>
            // GetAllIngredients: unit -> Async<Result<List<IngredientTO>, string>>
            AddTypeToIngredient : Guid * IngredientTypes -> Async<Result<unit, string>>
            GetDish: Guid -> Async<Result<DishTO, string>>
            AddTypeToDish : Guid * DishTypes -> Async<Result<unit, string>>
            RemoveTypeFromDish : Guid * DishTypes -> Async<Result<unit, string>>
            RemoveTypeFromIngredient : Guid * IngredientTypes -> Async<Result<unit, string>>
            GetDishes: unit -> Async<Result<List<DishTO>, string>>
        }