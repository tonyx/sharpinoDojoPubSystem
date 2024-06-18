module Tests

open Expecto
open System
open PubSystem.Kitchen
open Sharpino
open Sharpino.Core
open Sharpino.Utils
open Sharpino.Result
open Sharpino.CommandHandler
open Sharpino.Definitions
open FSharpPlus
open FsToolkit.ErrorHandling
open PubSystem.PubSystem
open PubSystem.Dishes
open Sharpino.PgStorage
open Sharpino.EventStore
open Sharpino
open Sharpino.MemoryStorage
open Sharpino.Storage
open Sharpino.TestUtils
open MBrace.FsPickler.Json
open PubSystem.Ingredients
open PubSystem.Shared.Definitions
open Sharpino.KafkaBroker
open Sharpino

let connection =
    "Server=127.0.0.1;" +
    "Database=es_orderssystem2;" +
    "User Id=safe;"+
    "Password=safe;"

// let pgEventStore = PgEventStore(connection)
let memoryEventStore:IEventStore<string> = MemoryStorage()
let pgEventStore:IEventStore<string> = PgEventStore(connection)

let doNothingBroker: IEventBroker<_> =
    {
        notify = None
        notifyAggregate = None
    }

let standardBroker = getKafkaBroker("localhost:9092")

let setUp (eventStore: IEventStore<string>) =
    eventStore.Reset Kitchen.Version Kitchen.StorageName
    eventStore.Reset Dish.Version Dish.StorageName
    eventStore.ResetAggregateStream Dish.Version Dish.StorageName
    eventStore.Reset Ingredient.Version Ingredient.StorageName
    eventStore.ResetAggregateStream Ingredient.Version Ingredient.StorageName
    Sharpino.Cache.StateCache<Kitchen>.Instance.Clear()
    Sharpino.Cache.AggregateCache<Dish, string>.Instance.Clear()
    Sharpino.Cache.AggregateCache<Ingredient, string>.Instance.Clear()

let pubSystems =
    [
        // PubSystem(memoryEventStore), memoryEventStore, "in memory event store test"
        PubSystem(pgEventStore, standardBroker), pgEventStore, "postgres eventstore test"
    ]

[<Tests>]
let tests =
  testList "samples" [
    multipleTestCase "initial state pub system has zero dish references - OK " pubSystems <| fun (pubSystem, eventStore, msg) ->
        setUp eventStore

        let dishRefs = pubSystem.GetDishReferences()
        Expect.isOk dishRefs "should be ok"
        let dishes = dishRefs.OkValue
        Expect.equal dishes.Length 0 "should be empty"

    multipleTestCase "initial state pub system, add a new dish - Ok" pubSystems <| fun (pubSystem, eventStore, msg) ->
        setUp eventStore

        let dishGuid = Guid.NewGuid()
        let dish = Dish (dishGuid, "dish1", [DishTypes.Main], [])
        let dishAdded = pubSystem.AddDish dish
        Expect.isOk dishAdded "should be ok"
        let dishes = pubSystem.GetDishReferences().OkValue
        Expect.equal dishes.Length 1 "should have one dish"

    multipleTestCase "initial state pub system, add a dish and retrieve it" pubSystems <| fun (pubSystem, eventStore, msg) ->
        setUp eventStore

        let dishGuid = Guid.NewGuid()
        let dish = Dish(dishGuid, "dish1", [DishTypes.Main], [])
        let dishAdded = pubSystem.AddDish(dish)
        Expect.isOk dishAdded "should be ok"

        let retrievedDish = pubSystem.GetDish dishGuid
        Expect.isOk retrievedDish "should be ok"
        Expect.equal retrievedDish.OkValue.Id dishGuid "should be the same dish"
        Expect.equal retrievedDish.OkValue.Name "dish1" "should have the same name"

    multipleTestCase "update dish name" pubSystems <| fun (pubSystem, eventStore, msg) ->
        setUp eventStore

        let dishGuid = Guid.NewGuid()
        let dish = Dish(dishGuid, "dish1", [DishTypes.Main], [])
        let dishAdded = pubSystem.AddDish dish
        Expect.isOk dishAdded "should be ok"

        let updatedDish = pubSystem.UpdateDishName (dishGuid, "dish2")
        Expect.isOk updatedDish "should be ok"
        let retrievedDish = pubSystem.GetDish dishGuid
        Expect.isOk retrievedDish "should be ok"
        Expect.equal retrievedDish.OkValue.Name "dish2" "should have the updated name"

    multipleTestCase "add two dishes and retrive their refs - OK" pubSystems <| fun (pubSystem, eventStore, msg) -> 
        setUp eventStore

        let dishGuid1 = Guid.NewGuid()
        let dishGuid2 = Guid.NewGuid()
        let dish1 = Dish(dishGuid1, "dish1", [DishTypes.Main], [])
        let dish2 = Dish(dishGuid2, "dish2", [DishTypes.Main], [])

        let dishAdded1 = pubSystem.AddDish dish1
        let dishAdded2 = pubSystem.AddDish dish2

        let retrieveRefs = pubSystem.GetDishReferences()
        Expect.equal retrieveRefs.OkValue.Length 2 "should have two dishes"

    multipleTestCase "Add two dishes and retrieve them - Ok" pubSystems <| fun (pubSystem, eventStore, _) ->
        setUp eventStore

        let dishGuid1 = Guid.NewGuid()
        let dishGuid2 = Guid.NewGuid()
        let dish1 = Dish(dishGuid1, "dish1", [DishTypes.Main], [])
        let dish2 = Dish(dishGuid2, "dish2", [DishTypes.Main], [])
        let dishAdded1 = pubSystem.AddDish(dish1)
        let dishAdded2 = pubSystem.AddDish(dish2)

        let retrievedDishes = pubSystem.GetDishes()
        let result = retrievedDishes |> Result.get

        Expect.equal result.Length 2 "should have two dishes"

        let ids = [dish1.Id; dish2.Id] |> Set.ofList
        let resultIds = result |> List.map (fun x -> x.Id) |> Set.ofList
        Expect.equal ids resultIds "should have the same ids"

    multipleTestCase "Add a dish and a dish Type to it" pubSystems <| fun (pubSystem, eventStore, _) ->
        setUp eventStore

        let dishguid = Guid.NewGuid()
        let dish = Dish(dishguid, "dish1", [DishTypes.Main], [])
        let dishType = DishTypes.Dessert

        let dishAdded = pubSystem.AddDish(dish)
        Expect.isOk dishAdded "should be ok"

        let updatedDish = pubSystem.AddTypesToDish(dishguid, dishType)
        Expect.isOk updatedDish "should be ok"

        let retrievedDish = pubSystem.GetDish dishguid
        let result = retrievedDish.OkValue
        Expect.equal result.DishTypes.Length 2 "should have two dish types"

    multipleTestCase "Remove a type to a dish - OK" pubSystems <| fun (pubSystem, eventStore, _) ->
        setUp eventStore

        let dishguid = Guid.NewGuid()
        let dish = Dish(dishguid, "dish1", [DishTypes.Main; DishTypes.Starter], [])
        let dishType = DishTypes.Dessert

        let dishAdded = pubSystem.AddDish(dish)
        Expect.isOk dishAdded "should be ok"

        let typeRemoved = pubSystem.RemoveTypeFromDish(dishguid, DishTypes.Main)

        let retrieved = pubSystem.GetDish dishguid
        let retrieved' = retrieved.OkValue

        Expect.equal retrieved'.DishTypes.Length 1 "should have one dish type"

    multipleTestCase "cannot remove a type to a dish having only one type - Error" pubSystems <| fun (pubSystem, eventStore, _) ->
        setUp eventStore

        let dishguid = Guid.NewGuid()
        let dish = Dish(dishguid, "dish1", [DishTypes.Main], [])

        let dishAdded = pubSystem.AddDish dish
        Expect.isOk dishAdded "should be ok"

        let typeRemoved = pubSystem.RemoveTypeFromDish(dishguid, DishTypes.Main)
        Expect.isError typeRemoved "should be error"

    multipleTestCase "add and retrieve an ingredient - Ok" pubSystems <| fun (pubSystem, eventStore, _) ->
        setUp eventStore

        let ingredientGuid = Guid.NewGuid()
        let ingredient = Ingredient(ingredientGuid, "ingredient1", [IngredientTypes.Dairy], [IngredientMeasures.Grams])
        let ingredientAdded = pubSystem.AddIngredient(ingredient)
        Expect.isOk ingredientAdded "should be ok"

        let retrievedIngredient = pubSystem.GetIngredient ingredientGuid
        Expect.isOk retrievedIngredient "should be ok"

        let allIngredients = pubSystem.GetAllIngredients()
        Expect.isOk allIngredients "should be ok"
        let ingredients = allIngredients.OkValue
        Expect.equal ingredients.Length 1 "should have one ingredient"

    multipleTestCase "add an ingredient and add a type to it - Ok " pubSystems <| fun (pubSystem, eventStore, _ ) ->
        setUp eventStore

        let ingredientGuid = Guid.NewGuid()
        let ingredient = Ingredient(ingredientGuid, "ingredient1", [IngredientTypes.Dairy], [IngredientMeasures.Grams])
        let ingredientAdded = pubSystem.AddIngredient ingredient
        Expect.isOk ingredientAdded "should be ok"

        let updatedIngredient = pubSystem.AddTypeToIngredient(ingredientGuid, IngredientTypes.Fish)
        Expect.isOk updatedIngredient "should be ok"

        let retrievedIngredient = pubSystem.GetIngredient ingredientGuid
        Expect.isOk retrievedIngredient "should be ok"

        let result = retrievedIngredient.OkValue
        Expect.equal result.IngredientTypes.Length 2 "should have two types"    

    multipleTestCase "add an ingredient and remove a type from it - Ok" pubSystems <| fun (pubSystem, eventStore, _) ->
        setUp eventStore

        let ingredientGuid = Guid.NewGuid()
        let ingredient = Ingredient(ingredientGuid, "ingredient1", [IngredientTypes.Dairy; IngredientTypes.Fish], [IngredientMeasures.Grams])
        let ingredientAdded = pubSystem.AddIngredient(ingredient)
        Expect.isOk ingredientAdded "should be ok"

        let updatedIngredient = pubSystem.RemoveTypeFromIngredient(ingredientGuid, IngredientTypes.Fish)
        Expect.isOk updatedIngredient "should be ok"

        let result = pubSystem.GetIngredient ingredientGuid
        Expect.isOk result "should be ok"
        Expect.equal result.OkValue.IngredientTypes.Length 1 "should have one type"

    multipleTestCase "add an ingredient and then create a dish with that ingredient - Ok" pubSystems <| fun (pubSystem, eventStore, _) ->
        setUp eventStore

        let ingredientGuid = Guid.NewGuid()
        let ingredient = Ingredient(ingredientGuid, "ingredient1", [IngredientTypes.Dairy; IngredientTypes.Fish], [IngredientMeasures.Grams])
        let ingredientAdded = pubSystem.AddIngredient(ingredient)
        Expect.isOk ingredientAdded "should be ok"

        let dishGuid = Guid.NewGuid()   
        let newDish = Dish(dishGuid, "dish1", [DishTypes.Main], [ingredientGuid])
        let added = pubSystem.AddDish newDish
        Expect.isOk added "should be ok"

        let retrieved = pubSystem.GetDish dishGuid
        Expect.isOk retrieved "should be ok"
        
        let result = retrieved.OkValue
        Expect.equal result.IngredientRefs.Length 1 "should have one ingredient"
    
    multipleTestCase "add an existing ingredient to an existing dishs - Ok" pubSystems <| fun (pubSystem, eventStore, _) ->
        setUp eventStore

        let ingredientGuid = Guid.NewGuid()
        let ingredient = Ingredient(ingredientGuid, "ingredient1", [IngredientTypes.Dairy; IngredientTypes.Fish], [IngredientMeasures.Grams])
        let ingredientAdded = pubSystem.AddIngredient(ingredient)
        Expect.isOk ingredientAdded "should be ok"

        let dishId = Guid.NewGuid()
        let dish = Dish(dishId, "dish1", [DishTypes.Main], [])

        let dishAdded = pubSystem.AddDish dish
        Expect.isOk dishAdded "should be ok"

        let updatedDish = pubSystem.AddIngredientToDish(dishId, ingredientGuid)
        Expect.isOk updatedDish "should be ok"

        let retrieved = pubSystem.GetDish dishId
        Expect.isOk retrieved "should be ok"

        let result = retrieved.OkValue
        let ingredientRefs = result.IngredientRefs
        Expect.equal ingredientRefs.Length 1 "should have one ingredient"
        Expect.equal ingredientRefs.[0] ingredientGuid "should have the same ingredient"

    multipleTestCase "add and remove a dish - Ok" pubSystems <| fun (pubSystem, eventStore, _) ->
        setUp eventStore

        let dishGuid = Guid.NewGuid()
        let dish = Dish(dishGuid, "dish1", [DishTypes.Main], [])
        let dishAdded = pubSystem.AddDish dish
        Expect.isOk dishAdded "should be ok"

        let removed = pubSystem.RemoveDish dishGuid
        Expect.isOk removed "should be ok"

        let retrieved = pubSystem.GetDish dishGuid
        Expect.isError retrieved "should be error"


    multipleTestCase "add a non existing ingredient to an existing dishs - Error" pubSystems <| fun (pubSystem, eventStore, _) ->
        setUp eventStore

        let dishId = Guid.NewGuid()
        let dish = Dish(dishId, "dish1", [DishTypes.Main], [])
        
        let dishAdded = pubSystem.AddDish dish

        let updatedDish = pubSystem.AddIngredientToDish(dishId, Guid.NewGuid())
        Expect.isError updatedDish "should be error"

  ]
  |> testSequenced
