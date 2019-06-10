module GrainInterfaces

open System.Threading.Tasks
open System

type ISimple =
    inherit Orleans.IGrainWithIntegerKey 
    abstract member SayHello : string -> Task<string>

type IPersistence =
    inherit Orleans.IGrainWithIntegerKey
    abstract member GetCreationTime: unit -> Task<DateTime>