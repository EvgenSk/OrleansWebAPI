namespace OrleansWebAPIFSharp

open System.Linq
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Console
open Microsoft.Extensions.DependencyInjection

open System.Reflection

open Orleans
open Orleans.Hosting

open Grains
open GrainInterfaces
open Microsoft.Extensions.Options

module Program =
    let exitCode = 0

    let getLoggerFactory () =
        let configureNamedOptions = ConfigureNamedOptions<ConsoleLoggerOptions>("", null);
        let optionsFactory = new OptionsFactory<ConsoleLoggerOptions>([|configureNamedOptions|], Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
        let optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(), new OptionsCache<ConsoleLoggerOptions>());

        let loggerFilterOptions = LoggerFilterOptions( MinLevel = LogLevel.Information )
        let codeGenLoggerFactory = new LoggerFactory([], loggerFilterOptions);

        codeGenLoggerFactory.AddProvider(new ConsoleLoggerProvider(optionsMonitor)) |> ignore
        codeGenLoggerFactory

    let CreateWebHostBuilder args =
        let assemblyInterfaces = Assembly.GetAssembly(typeof<ISimple>)
        let assemblyGrains = Assembly.GetAssembly(typeof<SimpleGrain>)

        use codeGenLoggerFactory = getLoggerFactory ()

        let silo = 
            SiloHostBuilder()
                .UseDashboard(fun o -> o |> ignore)
                .UseLocalhostClustering()
                .AddMemoryGrainStorage("OrleansStorage") // TODO: replace with your storage
                .ConfigureApplicationParts(fun parts -> 
                    parts.AddApplicationPart(assemblyInterfaces).WithCodeGeneration(codeGenLoggerFactory) |> ignore
                    parts.AddApplicationPart(assemblyGrains).WithCodeGeneration(codeGenLoggerFactory) |> ignore
                    )
                .Build();

        async {
            silo.StartAsync() 
            |> Async.AwaitIAsyncResult 
            |> Async.Ignore 
            |> ignore

            let client = silo.Services.GetRequiredService<IClusterClient>()

            return WebHost
                .CreateDefaultBuilder(args)
                .ConfigureServices(fun s -> 
                    s.AddSingleton<IGrainFactory>(client) |> ignore
                    s.AddSingleton<IClusterClient>(client) |> ignore)
                .UseStartup<Startup>();
        }
            

    [<EntryPoint>]
    let main args =
        async {
            let! webHostBuilder = CreateWebHostBuilder(args)
            webHostBuilder.Build().Run()
            return exitCode
        } |> Async.RunSynchronously
