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


[<Tests>]
let tests =
  testList "samples" [

    testCase "initial state pub system has zero dish references - OK " <| fun _ ->
        let pubSystem = PubSystem()  
        let dishRefs = pubSystem.GetDishReferences()
        Expect.isOk dishRefs "should be ok"
        let dishes = dishRefs.OkValue
        Expect.equal dishes.Length 0 "should be empty"

    testCase "initial state pub system, add a new dish - Ok" <| fun _ ->
        let pubSystem = PubSystem()
        let dishGuid = Guid.NewGuid()
        let dish = Dish(dishGuid, "dish1", [DishTypes.Main])
        let dishAdded = pubSystem.AddDish(dish)
        Expect.isOk dishAdded "should be ok"
        let dishes = pubSystem.GetDishReferences().OkValue
        Expect.equal dishes.Length 1 "should have one dish"

    testCase "initial state pub system, add a dish and retrieve it" <| fun _ ->
        let pubSystem = PubSystem()
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
