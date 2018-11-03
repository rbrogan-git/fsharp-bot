namespace EchoBotV4
open System.Threading.Tasks
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs
open System.Threading
open Microsoft.Bot.Schema
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Bot.Builder.Dialogs
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder
open Microsoft.Bot.Schema
open System.Threading.Tasks
open Microsoft.Bot.Builder

type EchoBot2(accessors:EchoBotAccessors) = 
    do printfn "In EchoBot init %A" accessors 
    let dialogs = new DialogSet (accessors.ConversationDialogState)
    do dialogs.Add(TextPrompt("name")) |> ignore
    let SendWelcomeMessageAsync (turnContext : ITurnContext) (cancellationToken: CancellationToken ) = 
        task{
            let reply = turnContext.Activity.CreateReply()
            reply.Text <- "welcome"
            do! turnContext.SendActivityAsync (reply, cancellationToken) :> Task
        }
        //     foreach (var member in turnContext.Activity.MembersAdded)
        //     {
        //         if (member.Id != turnContext.Activity.Recipient.Id)
        //         {
        //             var reply = turnContext.Activity.CreateReply();
        //             reply.Text = WelcomeText;
        //             await turnContext.SendActivityAsync(reply, cancellationToken);
        //         }
        //     }
        // }
    interface IBot with 
        member this.OnTurnAsync (turnContext : ITurnContext, cancellationToken : CancellationToken ) =
            task {
                match turnContext.Activity.Type with
                | ActivityTypes.Message ->
                    // Run the DialogSet - let the framework identify the current state of the dialog from
                    // the dialog stack and figure out what (if any) is the active dialog.
                    let! dialogContext = dialogs.CreateContextAsync(turnContext, cancellationToken)
                    let! results = dialogContext.ContinueDialogAsync(cancellationToken)
                    match results.Status with
                    | DialogTurnStatus.Empty -> 
                        do! dialogContext.PromptAsync(
                                "name", 
                                PromptOptions(Prompt = MessageFactory.Text "Please enter your name."),
                                cancellationToken
                            ) :> Task
                    | DialogTurnStatus.Complete ->  // We had a dialog run (it was the prompt). Now it is Complete.
                        //if (results.Result <> null) then 
                        do! turnContext.SendActivityAsync(
                                MessageFactory.Text (sprintf "Thank you, I have your name as %A" results.Result)
                            ) :> Task
 
                | ActivityTypes.ConversationUpdate ->
                    if (turnContext.Activity.MembersAdded <> null) then
                        do! SendWelcomeMessageAsync turnContext cancellationToken
                | _ ->
                    do! turnContext.SendActivityAsync(sprintf "{%s} event detected" turnContext.Activity.Type) :> Task  
                do! accessors.ConversationState.SaveChangesAsync (turnContext, false, cancellationToken)              
            } :> _
   