namespace EchoBotV4
open System.Threading.Tasks;
open Microsoft.Bot.Builder;
open System.Threading
open Microsoft.Bot.Schema
module Async =
    let AwaitTaskVoid : (Task -> Async<unit>) =
        Async.AwaitIAsyncResult >> Async.Ignore
type EchoBotOld(accessors:EchoBotAccessors) = 
    do printfn "In EchoBot init %A" accessors 
    interface IBot with 
        member this.OnTurnAsync (turnContext : ITurnContext, cancellationToken:  CancellationToken  ) =
            async {
                if (turnContext.Activity.Type = ActivityTypes.Message) then
                        // Get the conversation state from the turn context.
                        let! state = accessors.CounterState.GetAsync(turnContext, fun () -> new CounterState()) |> Async.AwaitTask  

                        // Bump the turn count for this conversation.
                        state.TurnCount <- state.TurnCount + 1

                        // Set the property using the accessor.
                        do! accessors.CounterState.SetAsync(turnContext, state) |> Async.AwaitTaskVoid

                        // Save the new turn count into the conversation state.
                        do! accessors.ConversationState.SaveChangesAsync(turnContext) |> Async.AwaitTaskVoid

                        // Echo back to the user whatever they typed.
                        let responseMessage = sprintf "Turn %A: You sent '%A'\n" state.TurnCount turnContext.Activity.Text
                        do! turnContext.SendActivityAsync(responseMessage)  |> Async.AwaitTaskVoid
                    else
                        do! turnContext.SendActivityAsync(sprintf "%A event detected" turnContext.Activity.Type)  |> Async.AwaitTaskVoid
            } |> Async.StartAsTask :> Task
    member this.testMethod (turnContext : ITurnContext, cancellationToken:  CancellationToken  ) =
        async {
            if (turnContext.Activity.Type = ActivityTypes.Message) then
                    // Get the conversation state from the turn context.
                    let! state = accessors.CounterState.GetAsync(turnContext, fun () -> new CounterState()) |> Async.AwaitTask  

                    // Bump the turn count for this conversation.
                    state.TurnCount <- state.TurnCount + 1

                    // Set the property using the accessor.
                    do! accessors.CounterState.SetAsync(turnContext, state) |> Async.AwaitTaskVoid

                    // Save the new turn count into the conversation state.
                    do! accessors.ConversationState.SaveChangesAsync(turnContext) |> Async.AwaitTaskVoid

                    // Echo back to the user whatever they typed.
                    let responseMessage = sprintf "Turn %A: You sent '%A'\n" state.TurnCount turnContext.Activity.Text
                    do! turnContext.SendActivityAsync(responseMessage)  |> Async.AwaitTaskVoid
                else
                    do! turnContext.SendActivityAsync(sprintf "%A event detected" turnContext.Activity.Type)  |> Async.AwaitTaskVoid
        } |> Async.StartAsTask :> Task

    member this.OnTurnAsync2 (turnContext : ITurnContext, cancellationToken:  CancellationToken  ) =
        match turnContext.Activity.Type with
        | ActivityTypes.Message ->
                
            turnContext.SendActivityAsync("Hi from F#!!") |> ignore
            let state = 
                accessors.CounterState.GetAsync(turnContext,fun () -> new CounterState()) 
                |> Async.AwaitTask
                |> Async.RunSynchronously

            // Bump the turn count for this conversation.
            state.TurnCount <- state.TurnCount + 1

            // Set the property using the accessor.
            accessors.CounterState.SetAsync(turnContext, state) |> ignore

            // Save the new turn count into the conversation state.
            accessors.ConversationState.SaveChangesAsync(turnContext) |> ignore

            // Echo back to the user whatever they typed.
            let responseMessage = sprintf "Turn %i: You sent '%s'\n" state.TurnCount turnContext.Activity.Text
            turnContext.SendActivityAsync(responseMessage) :> Task
        | _ ->
            turnContext.SendActivityAsync(sprintf "{%s} event detected" turnContext.Activity.Type) :> Task