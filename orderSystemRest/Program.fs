// For more information see https://aka.ms/fsharp-console-apps
namespace PubSystem.Server
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open PubSystem.Shared
open Saturn.Application
open Saturn
open PubSystem.Shared.Definitions
open PubSystem.PubSystem
open PubSystem.Shared.Services
open Sharpino.PgStorage

module restService =
    open PubSystem.Dishes
    open PubSystem.Ingredients
    let connection =
        "Server=127.0.0.1;" +
        "Database=es_orderssystem2;" +
        "User Id=safe;"+
        "Password=safe;"


    let eventStore = PgEventStore(connection)

    let pubSystem = PubSystem(eventStore)

    let pubSystemRest: IRestPubSystem = 
        {
            AddDish = 
                fun dish -> async {
                    return
                        pubSystem.AddDish (Dish.FromDishTO dish)
                        |> Result.map (fun _ -> ())
                }
            AddIngredient = 
                fun ingredient -> async {
                    return
                        pubSystem.AddIngredient (Ingredient.FromIngredientTO ingredient)
                        |> Result.map (fun _ -> ())
                }

            GetIngredients = 
                fun () -> async {
                    return
                        pubSystem.GetAllIngredients ()
                        |> Result.map (fun ingredients -> ingredients |> List.map (fun x -> x.ToIngredienTO))
                }

            UpdateDishName = 
                fun (dishId, name) -> async {
                    return
                        pubSystem.UpdateDishName (dishId, name)
                        |> Result.map (fun _ -> ())
                }
            AddTypeToIngredient = 
                fun (ingredientId, ingredientType) -> async {
                    return
                        pubSystem.AddTypeToIngredient (ingredientId, ingredientType)
                        |> Result.map (fun _ -> ())
                }
            GetDish = 
                fun dishId -> async {
                    return
                        pubSystem.GetDish (dishId)
                        |> Result.map (fun dish -> dish.ToDishTO)
                }
            AddTypeToDish = 
                fun (dishId, dishType) -> async {
                    return
                        pubSystem.AddTypesToDish (dishId, dishType)
                        |> Result.map (fun _ -> ())
                }
            RemoveTypeFromDish = 
                fun (dishId, dishType) -> async {
                    return
                        pubSystem.RemoveTypeFromDish (dishId, dishType)
                        |> Result.map (fun _ -> ())
                }
            RemoveTypeFromIngredient =
                fun (ingredientId, ingredientType) -> async {
                    return
                        pubSystem.RemoveTypeFromIngredient (ingredientId, ingredientType)
                        |> Result.map (fun _ -> ())
                }
            GetDishes = 
                fun () -> async {
                    return
                        pubSystem.GetDishes ()
                        |> Result.map (fun dishes -> dishes |> List.map (fun x -> x.ToDishTO))
                }
        }

    let webApp =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromValue pubSystemRest
        |> Remoting.buildHttpHandler

    let app = application {
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

    [<EntryPoint>]
    let main _ =
        run app
        0