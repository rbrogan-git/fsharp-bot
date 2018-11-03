namespace EchoBotV4
open Microsoft.Bot.Builder
open Microsoft.Bot.Builder.Dialogs

type CounterState() =   
        member val TurnCount : int = 0 with get, set
    
type EchoBotAccessors (conversationState : ConversationState ) =
        
        static member CounterStateName  = "{nameof(EchoBotAccessors)}.CounterState" 
   
        member val CounterState :IStatePropertyAccessor<CounterState> = null with  get, set
        member val ConversationDialogState :IStatePropertyAccessor<DialogState> = null with  get, set

        member val ConversationState : ConversationState  = conversationState with  get
    