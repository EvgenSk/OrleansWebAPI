namespace OrleansWebAPIFSharp.Controllers

open Microsoft.AspNetCore.Mvc

open GrainInterfaces
open Orleans

[<Route("api/[controller]")>]
[<ApiController>]
type PersistenceController (client: IClusterClient) =
    inherit ControllerBase()

    [<HttpGet("{id}")>]
    member this.Get(id:int) =
        let grain = client.GetGrain<IPersistence> <| int64 id 
        async {
            let! creationTime = grain.GetCreationTime() |> Async.AwaitTask
            let message = sprintf "My ID is %d. I was created at %s" id <| creationTime.ToShortTimeString()
            return ActionResult<string>(message)
        }