using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;
using FChatDicebot.SavedData;

namespace FChatDicebot.BotCommands.Base
{
    public class SendCommandToGame
    {
        public static void Run(BotMain bot, BotCommandController commandController,
            string[] rawTerms, string[] terms, string characterName, string channel,
            UserGeneratedCommand command, string originCommandName)
        {
            //sent from dice bot in private - figure out the channel somehow
            
            //query existing RPS games to find where they're a player?
            GameSession relevantSession = bot.DiceBot.GameSessions.FirstOrDefault(a => a.CurrentGame.GetGameName() == "RockPaperScissors"
                && a.RockPaperScissorsData.RockPaperScissorsPlayers != null
                && a.RockPaperScissorsData.CurrentGamePhase == RockPaperScissorsGamePhase.WaitingForThrows
                && a.RockPaperScissorsData.RockPaperScissorsPlayers.Count(b => b.PlayerName == characterName) > 0);
            //apply this command to existing RPS game
            string responseMessage = "";
            if(relevantSession == null)
            {
                responseMessage = "Error: no relevant game session found";
            }
            else
            {
                GameSymbol usedSymbol = GameSymbol.NONE;
                //get symbol from inputs
                if (originCommandName == "rock")//terms.Contains("rock"))
                    usedSymbol = GameSymbol.Rock;
                else if (originCommandName == "paper")//(terms.Contains("paper"))
                    usedSymbol = GameSymbol.Paper;
                else if (originCommandName == "scissors")//(terms.Contains("scissors"))
                    usedSymbol = GameSymbol.Scissors;
                else if (originCommandName == "lizard")
                    usedSymbol = GameSymbol.Lizard;
                else if (originCommandName == "spock")
                    usedSymbol = GameSymbol.Spock;

                RockPaperScissors sessionOfRps = (RockPaperScissors) relevantSession.CurrentGame;
                sessionOfRps.ApplySymbolToPlayer(bot, relevantSession, characterName, usedSymbol);
                responseMessage += "You've sent a " + sessionOfRps.GetThrowString(usedSymbol) + "!";
            }

            if (commandController.MessageCameFromChannel(channel))
            {
                bot.SendMessageInChannel(responseMessage, channel);
            }
            else
            {
                bot.SendPrivateMessage(responseMessage, characterName);
            }
        }
    }
}
