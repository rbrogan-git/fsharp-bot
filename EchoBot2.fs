namespace EchoBotV4
open System.Threading.Tasks;
open Microsoft.Bot.Builder;
open System.Threading
open Microsoft.Bot.Schema
open FSharp.Control.Tasks.V2

type EchoBot2(accessors:EchoBotAccessors) = 
    do printfn "In EchoBot init %A" accessors 
    interface IBot with 
        member this.OnTurnAsync (turnContext : ITurnContext, cancellationToken : CancellationToken ) =
            task {
                match turnContext.Activity.Type with
                | ActivityTypes.Message ->
                
                    do! turnContext.SendActivityAsync("Hi from F#!!") :> Task
                    let! state = accessors.CounterState.GetAsync(turnContext,fun () -> new CounterState()) 
 
                    // Bump the turn count for this conversation.
                    state.TurnCount <- state.TurnCount + 1

                    // Set the property using the accessor.
                    do! accessors.CounterState.SetAsync(turnContext, state) 

                    // Save the new turn count into the conversation state.
                    do! accessors.ConversationState.SaveChangesAsync(turnContext) 

                    // Echo back to the user whatever they typed.
                    let responseMessage = sprintf "Turn %i: You sent '%s'\n" state.TurnCount turnContext.Activity.Text
                    do! turnContext.SendActivityAsync(responseMessage) :> Task
                | _ ->
                    do! turnContext.SendActivityAsync(sprintf "{%s} event detected" turnContext.Activity.Type) :> Task     
            } :> _
   