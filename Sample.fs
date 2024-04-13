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


let connection =
    "Server=127.0.0.1;" +
    "Database=es_orderssystem2;" +
    "User Id=safe;"+
    "Password=safe;"

// let pgEventStore = PgEventStore(connection)
let memoryEventStore:IEventStore = MemoryStorage()
let pgEventStore:IEventStore = PgEventStore(connection)

let setUp (eventStore: IEventStore) =
    eventStore.Reset Kitchen.Version Kitchen.StorageName
    eventStore.ResetAggregateStream Dish.Version Dish.StorageName

let pubSystems =
    [
        PubSystem(memoryEventStore), memoryEventStore, "questo e' basato sulla memoria"
        // PubSystem(pgEventStore), pgEventStore, "questo e' il test basto su postgres"
    ]

[<Tests>]
let tests =
  testList "samples" [
    multipleTestCase "initial state pub system has zero dish references - OK " pubSystems <| fun (pubSystem, eventStore, msg) ->
        setUp eventStore

        printf "%s\n" msg
        let dishRefs = pubSystem.GetDishReferences()
        Expect.isOk dishRefs "should be ok"
        let dishes = dishRefs.OkValue
        Expect.equal dishes.Length 0 "should be empty"

    multipleTestCase "initial state pub system, add a new dish - Ok" pubSystems <| fun (pubSystem, eventStore, msg) ->
        setUp eventStore

        printf "%s\n" msg
        let dishGuid = Guid.NewGuid()
        let dish = Dish(dishGuid, "dish1", [DishTypes.Main])
        let dishAdded = pubSystem.AddDish(dish)
        Expect.isOk dishAdded "should be ok"
        let dishes = pubSystem.GetDishReferences().OkValue
        Expect.equal dishes.Length 1 "should have one dish"

    multipleTestCase "initial state pub system, add a dish and retrieve it" pubSystems <| fun (pubSystem, eventStore, msg) ->
        setUp eventStore

        printf "%s\n" msg
        let dishGuid = Guid.NewGuid()
        let dish = Dish(dishGuid, "dish1", [DishTypes.Main])
        let dishAdded = pubSystem.AddDish(dish)
        Expect.isOk dishAdded "should be ok"

        let retrievedDish = pubSystem.GetDish dishGuid
        Expect.isOk retrievedDish "should be ok"
        Expect.equal retrievedDish.OkValue.Id dishGuid "should be the same dish"
        Expect.equal retrievedDish.OkValue.Name "dish1" "should have the same name"
  ]
  |> testSequenced
