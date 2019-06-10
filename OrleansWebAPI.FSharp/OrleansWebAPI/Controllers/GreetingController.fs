namespace OrleansWebAPIFSharp.Controllers

open Microsoft.AspNetCore.Mvc
open Orleans

open GrainInterfaces

[<Route("api/[controller]")>]
[<ApiController>]
type GreetingController (client: IClusterClient) =
    inherit ControllerBase()

    [<HttpGet("{name}")>]
    member this.Get(name:string) =
        let grain = client.GetGrain<ISimple> <| int64 0 
        async {
            let! greeting = grain.SayHello(name) |> Async.AwaitTask
            return ActionResult<string>(greeting)
        }