using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.DiceFunctions
{
    public class GameSession
    {
        public string ChannelId;

        public IGame CurrentGame;

        public GameState State;

        public List<String> Players;
        //TODO: add to the game sessions a way for them to store their own data or interpret a shared field here
        public List<RouletteBetData> RouletteBets = new List<RouletteBetData>();
        public List<KingsGamePlayer> KingsGamePlayers = new List<KingsGamePlayer>();

        public string PokerTurnPlayer;

        public int Ante;
        public bool AnteSet = false;

        public int MinimumBetIncrement;

        public string RunGame(DiceBot diceBot, BotMain botMain)
        {
            string returnString = CurrentGame.RunGame(diceBot.random, Players, diceBot, botMain, this);

            return returnString;
        }

        public string IssueGameCommand(DiceBot diceBot, BotMain botMain, string character, string channel, string[] terms)
        {
            string returnString = CurrentGame.IssueGameCommand(diceBot, botMain, character, channel, this, terms);

            return returnString;
        }

        public string GetStatus(double secondOnTimer = 0)
        {
            string readyToStartString = "";
            if (Players.Count >= CurrentGame.GetMinPlayers() && secondOnTimer == 0)
            {
                readyToStartString += "[b] (Ready to start!)[/b]";
            }
            else if(secondOnTimer > 0)
            {
                readyToStartString += "[i] (" + secondOnTimer.ToString("N3") + " seconds until ready to start)[/i]";
            }

            string betString = "";
            if(RouletteBets != null && RouletteBets.Count > 0)
            {
                foreach(RouletteBetData bet in RouletteBets)
                {
                    if (!string.IsNullOrEmpty(betString))
                    {
                        betString += ", ";
                    }

                    betString += bet.GetBetString();
                }
                betString = "\nCurrent Bets: " + betString;
            }

            return string.Format("[b]Game Name: {0}[/b] \nMin Players: {1} Max Players: {2} Ante: {3}\n" + 
                "Game State: [b]{4}[/b]\nCurrent Players: {5}{6}" + 
                "{7}", 
                CurrentGame.GetGameName(), CurrentGame.GetMinPlayers(), CurrentGame.GetMaxPlayers(), Ante,
                State.ToString(), Utils.PrintList(Players), readyToStartString, betString);
        }

        public bool AddGameData(object gameDataObject)
        {
            if (typeof(RouletteBetData) == gameDataObject.GetType())
            {
                RouletteBets.Add((RouletteBetData)gameDataObject);
                return true;
            }
            else
                return false;
        }

        public bool RemoveRouletteBet(string characterName)
        {
            if(RouletteBets != null && RouletteBets.Count > 0)
                RouletteBets.RemoveAll(a => a.characterName == characterName);

            return true;
        }
    }

    public enum GameState
    {
        NONE,
        Unstarted,
        Started,
        Waiting,
        Finished,
        GameInProgress
    }

    public enum ContinueGameType
    {
        NONE,
        AwardPointsKingsGame
    }
}



