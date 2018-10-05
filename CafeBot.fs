namespace EchoBotV4(* 
open Microsoft.Bot;
open Microsoft.Bot.Builder;
open Microsoft.Bot.Builder.Core.Extensions;
open Microsoft.Bot.Builder.Dialogs;
open Microsoft.Bot.Builder.Prompts;
open Microsoft.Bot.Schema;
open Microsoft.Recognizers.Text;
open System;
open System.Collections.Generic;
open System.Linq;
open System.Threading.Tasks;


module CafeBot = 
    : IBot
    {
        private readonly DialogSet dialogs;
        static DateTime reservationDate;
        static int partySize;
        static string reservationName;
        public CafeBot()
        {
            dialogs = new DialogSet();

            dialogs.Add("reserveTable", new WaterfallStep[] {

                async (dc, args, next) => {
                    await dc.Context.SendActivity("Welcome to the res service!");
                    dc.ActiveDialog.State = new Dictionary<string, object>();
                    await dc.Prompt("dateTimePrompt","What is the date and time for the reservation?");

                },
                async (dc, args, next) => {
                    var dateTimeResult = ((DateTimeResult)args).Resolution.First();
                    reservationDate = Convert.ToDateTime(dateTimeResult.Value);
                    dc.ActiveDialog.State["date"] = Convert.ToDateTime(dateTimeResult.Value);
                    await dc.Prompt("partySizePrompt", "How many people in your party?");
                },
                async (dc, args, next) => {
                    partySize = (int)args["Value"];
                    dc.ActiveDialog.State["partySize"] = (int)args["Value"];
                    await dc.Prompt("textPrompt", "Whose name will this be under?");
                },
                async (dc, args, next) => {
                    reservationName = args["Text"].ToString();
                     dc.ActiveDialog.State["name"] = args["Text"];
                    string msg = "Reservation confirmed. Reservation details - " +
                        $"\nDate/Time: {reservationDate.ToString()} " +
                        $"\nParty size: {partySize.ToString()} " +
                        $"\nReservation name: {reservationName}";

                     var convo = ConversationState<Dictionary<string, object>>.Get(dc.Context);
                     // In production, you may want to store something more helpful
                    convo[$"{dc.ActiveDialog.State["name"]} reservation"] = msg;
                    await dc.Context.SendActivity(msg);
                    await dc.End();
             
                }


            });
            dialogs.Add("dateTimePrompt", new Microsoft.Bot.Builder.Dialogs.DateTimePrompt(Culture.English));
            // Add a prompt for the party size
            dialogs.Add("partySizePrompt", new Microsoft.Bot.Builder.Dialogs.NumberPrompt<int>(Culture.English));
            // Add a prompt for the user's name
            dialogs.Add("textPrompt", new Microsoft.Bot.Builder.Dialogs.TextPrompt());
        }

        public async Task OnTurn(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                var state = ConversationState<Dictionary<string, object>>.Get(context);
                var dc = dialogs.CreateContext(context, state);
                await dc.Continue();
                if (!context.Responded)
                {
                    if (context.Activity.Text.ToLowerInvariant().Contains("reserve table"))
                    {
                        await dc.Begin("reserveTable");
                    }
                    else
                    {
                        await context.SendActivity($"You said '{context.Activity.Text}'");
                    }

                }
            }
        }
    }
}
*)