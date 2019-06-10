module Grains

open GrainInterfaces
open System.Threading.Tasks
open Orleans.Providers
open System

type SimpleGrain() =
    inherit Orleans.Grain()
    interface ISimple with 
        member x.SayHello (greeting: string) = 
            Task.FromResult <| sprintf "Client said '%s', so the SimpleGrain answers 'Hello!'" greeting
    
type SimpleType(creationTime: DateTime) =
    member this.CreationTime = creationTime
    new() = SimpleType(DateTime.UtcNow)

[<StorageProvider(ProviderName="OrleansStorage")>]
type PersistenceGrain() =
    inherit Orleans.Grain<SimpleType>()
    interface IPersistence with 
        member x.GetCreationTime () = Task.FromResult x.State.CreationTime
