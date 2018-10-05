namespace EchoBotV4
open System;
open System.Linq;
open Microsoft.AspNetCore.Builder;
open Microsoft.AspNetCore.Hosting;
open Microsoft.Bot.Builder;
open Microsoft.Bot.Builder.Integration;
open Microsoft.Bot.Builder.Integration.AspNet.Core;
open Microsoft.Bot.Configuration;
open Microsoft.Bot.Connector.Authentication;
open Microsoft.Extensions.Configuration;
open Microsoft.Extensions.DependencyInjection;
open Microsoft.Extensions.Logging;
open Microsoft.Extensions.Options;


type Startup private () =
    new ( env : IHostingEnvironment) as this =
        Startup() then
        let builder = new ConfigurationBuilder()
        builder.SetBasePath(env.ContentRootPath) |> ignore
        builder.AddJsonFile("appsettings.json", optional= true, reloadOnChange= true) |> ignore
        builder.AddJsonFile("appsettings." + env.EnvironmentName + ".json", optional= true) |> ignore
        builder.AddEnvironmentVariables() |> ignore
        let config = builder.Build() :> IConfiguration
        this.Configuration <- config

        

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940


        // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) = 
            //let botFramOpts = new ConfigurationCredentialProvider(this.Configuration)
   
            services.AddBot<EchoBot>(fun options -> 
                                let secretKey = this.Configuration.["botFileSecret"]
                                let botFilePath = this.Configuration.["botFilePath"]
                                let env = this.Configuration.["environment"]
                                
                                let dataStore = new MemoryStorage() :> IStorage
                                let conversationState = new ConversationState(dataStore)
                                options.State.Add(conversationState) |> ignore
                                // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
                                let botConfig = BotConfiguration.Load(botFilePath , secretKey);
                                services.AddSingleton<BotConfiguration>(botConfig) |> ignore
                
                                let service = botConfig.Services.Where(fun s -> s.Type = "endpoint" && s.Name = env).FirstOrDefault() :?> EndpointService
                                options.CredentialProvider <- new SimpleCredentialProvider(service.AppId, service.AppPassword) 
                                ()
                                ) |> ignore
                        // Acessors created here are passed into the IBot-derived class on every turn.
            services.AddSingleton<EchoBotAccessors>(fun sp ->
                let options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value
                if (options = null) then 
                    failwith "BotFrameworkOptions must be configured prior to setting up the state accessors"
                

                let conversationState = options.State.OfType<ConversationState>().FirstOrDefault()
                if (conversationState = null) then
                    failwith "ConversationState must be defined and added before adding conversation-scoped state accessors."

                // Create the custom state accessor.
                // State accessors enable other components to read and write individual properties of state.
                let accessors = new EchoBotAccessors(conversationState)             
                accessors.CounterState <- conversationState.CreateProperty<CounterState>(EchoBotAccessors.CounterStateName)
                accessors) |> ignore
 
            //services.AddBot<EchoBot>(fun options ->
                //options.CredentialProvider <- botFramOpts

                // The CatchExceptionMiddleware provides a top-level exception handler for your bot. 
                // Any exceptions thrown by other Middleware, or by your OnTurn method, will be 
                // caught here. To facillitate debugging, the exception is sent out, via Trace, 
                // to the emulator. Trace activities are NOT displayed to users, so in addition
                // an "Ooops" message is sent. 
                //options.Middleware.Add(new CatchExceptionMiddleware<Exception>(async (context, exception) =>
                //{
                //    await context.TraceActivity("EchoBot Exception", exception);
                //    await context.SendActivity("Sorry, it looks like something went wrong!");
                //}));

                

                // For production bots use the Azure Blob or
                // Azure CosmosDB storage providers. For the Azure
                // based storage providers, add the Microsoft.Bot.Builder.Azure
                // Nuget package to your solution. That package is found at:
                // https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/
                // Uncomment the following lines to use Azure Blob Storage
                // //Storage configuration name or ID from the .bot file.
                // const string StorageConfigurationId = "<STORAGE-NAME-OR-ID-FROM-BOT-FILE>";
                // var blobConfig = botConfig.FindServiceByNameOrId(StorageConfigurationId);
                // if (!(blobConfig is BlobStorageService blobStorageConfig))
                // {
                //    throw new InvalidOperationException($"The .bot file does not contain an blob storage with name '{StorageConfigurationId}'.");
                // }
                // // Default container name.
                // const string DefaultBotContainer = "<DEFAULT-CONTAINER>";
                // var storageContainer = string.IsNullOrWhiteSpace(blobStorageConfig.Container) ? DefaultBotContainer : blobStorageConfig.Container;
                // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage(blobStorageConfig.ConnectionString, storageContainer);

                // Create Conversation State object.
                // The Conversation State object is where we persist anything at the conversation-scope.
                
           
        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
            if (env.IsDevelopment()) then          
                app.UseDeveloperExceptionPage() |> ignore
            

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework() |> ignore
           
    member val Configuration : IConfiguration = null with get, set
