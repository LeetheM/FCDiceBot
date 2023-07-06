using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.DiceFunctions
{
    public class HighRoll : IGame
    {
        public string GetGameName()
        {
            return "HighRoll";
        }

        public int GetMaxPlayers()
        {
            return 20;
        }

        public int GetMinPlayers()
        {
            return 2;
        }

        public bool AllowAnte()
        {
            return true;
        }

        public bool UsesFlatAnte()
        {
            return true;
        }

        public bool KeepSessionDefault()
        {
            return false;
        }

        public int GetMinimumMsBetweenGames()
        {
            return 0;
        }

        public string GetStartingDisplay()
        {
            return "[eicon]dbhighroll1b[/eicon][eicon]dbhighroll2b[/eicon]";
        }

        public string GetEndingDisplay()
        {
            return "";
        }

        public string GameStatus(GameSession session)
        {
            return "";
        }

        public bool AddGameDataForPlayerJoin(string characterName, GameSession session, BotMain botMain, string[] terms, int ante, out string messageString)
        {
            messageString = "";
            return true;
        }

        public string PlayerLeftGame(BotMain botMain, GameSession session, string characterName)
        {
            return "";
        }

        public string RunGame(System.Random r, String executingPlayer, List<String> playerNames, DiceBot diceBot, BotMain botMain, GameSession session)
        {
            List<string> currentPlayerNames = new List<string>();

            foreach(string s in playerNames)
            {
                currentPlayerNames.Add(s);
            }

            string outputString = "";
            bool highestRollFound = false;
            bool addedAntes = false;

            while (!highestRollFound)
            {
                int highestRoll = 0;
                List<DiceRoll> finishedRolls = new List<DiceRoll>();

                foreach (string s in currentPlayerNames)
                {
                    DiceRoll d = new DiceRoll() { DiceRolled = 1, DiceSides = 100 };

                    d.Roll(r);

                    finishedRolls.Add(d);
                }

                for (int i = 0; i < currentPlayerNames.Count; i++)
                {
                    if (!string.IsNullOrEmpty(outputString))
                    {
                        outputString += "\n";
                    }

                    string betstring = "";

                    if(!addedAntes && session.Ante > 0)
                    {
                        betstring = diceBot.BetChips(currentPlayerNames[i], session.ChannelId, session.Ante, false) + "\n";
                    }

                    string thisPlayerRollString = betstring + "[i]" + Utils.GetCharacterUserTags(currentPlayerNames[i]) + "[/i] rolling: " + finishedRolls[i].ResultString();
                    outputString += thisPlayerRollString;

                    if (highestRoll < finishedRolls[i].Total)
                    {
                        highestRoll = (int) finishedRolls[i].Total;
                    }
                }

                addedAntes = true;

                //reroll if there's a tie
                if(finishedRolls.Where(a => a.Total == highestRoll).Count() > 1)
                {
                    outputString += "\n[color=yellow]Tie roll![/color] Rerolling...";
                    //get indexes of highest rollers and remove everyone else
                    List<int> removeIndex = new List<int>();
                    for(int i =0; i < currentPlayerNames.Count; i++)
                    {
                        if(finishedRolls[i].Total != highestRoll)
                        {
                            removeIndex.Add(i);
                        }
                    }
                    if(removeIndex.Count > 0)
                    {
                        removeIndex = removeIndex.OrderByDescending(a => a).ToList();
                        foreach(int removed in removeIndex)
                        {
                            currentPlayerNames.RemoveAt(removed);
                            finishedRolls.RemoveAt(removed);
                        }
                    }
                }
                    //end the loop because a highest roll was found
                else
                {
                    highestRollFound = true;

                    DiceRoll highestDiceRoll = finishedRolls.FirstOrDefault(a => a.Total == highestRoll);
                    int highestDiceRollIndex = finishedRolls.IndexOf(highestDiceRoll);

                    string playerNameWinner = currentPlayerNames[highestDiceRollIndex];

                    string betstring = "";
                    if(session.Ante > 0)
                    {
                        betstring = "\n" + diceBot.ClaimPot(playerNameWinner, session.ChannelId, 1);
                    }

                    outputString += "\n[b]" + Utils.GetCharacterUserTags(playerNameWinner) + " wins![/b]" + betstring;
                }
            }

            session.State = DiceFunctions.GameState.Finished;

            return outputString;
        }

        public string IssueGameCommand(DiceBot diceBot, BotMain botMain, string character, string channel, GameSession session, string[] terms, string[] rawTerms)
        {
            return GetGameName() + " has no valid GameCommands";
        }
    }
}
