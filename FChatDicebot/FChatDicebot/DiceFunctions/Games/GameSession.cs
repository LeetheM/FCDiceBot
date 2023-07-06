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
        public PokerGameData PokerGameData = new PokerGameData();
        public BlackjackGameData BlackjackGameData = new BlackjackGameData();
        public LiarsDiceData LiarsDiceData = new LiarsDiceData();
        public SlamRollData SlamRollData = new SlamRollData();
        public RockPaperScissorsData RockPaperScissorsData = new RockPaperScissorsData();
        public PokerData PokerData = new PokerData();

        public PlayerRandomQueueData RandomPlayerQueueData = new PlayerRandomQueueData();

        //public GameData GameData = new GameData();

        public int Ante;
        public bool AnteSet = false;

        public int MinimumBetIncrement;

        public string RunGame(String executingCharacter, DiceBot diceBot, BotMain botMain)
        {
            string returnString = CurrentGame.RunGame(diceBot.random, executingCharacter, Players, diceBot, botMain, this);

            return returnString;
        }

        public string IssueGameCommand(DiceBot diceBot, BotMain botMain, string character, string channel, string[] terms, string[] rawTerms)
        {
            string returnString = CurrentGame.IssueGameCommand(diceBot, botMain, character, channel, this, terms, rawTerms);

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
            
            string gameRelatedInfo = CurrentGame.GameStatus(this);
            if (!string.IsNullOrEmpty(gameRelatedInfo))
                gameRelatedInfo = "\n" + gameRelatedInfo;

            return string.Format("[b]Game Name: {0}[/b] \nMin Players: {1}, Max Players: {2}, Keep Session: {3}, Ante: {4}\n" + 
                "Game State: [b]{5}[/b]\nCurrent Players: {6}{7}" + 
                "{8}", 
                CurrentGame.GetGameName(), CurrentGame.GetMinPlayers(), CurrentGame.GetMaxPlayers(), CurrentGame.KeepSessionDefault(), Ante,
                State.ToString(), Utils.PrintList(Players), readyToStartString, gameRelatedInfo);
        }

        public bool HasEnoughPlayersToStart()
        {
            return Players != null && CurrentGame != null && Players.Count >= CurrentGame.GetMinPlayers();
        }

        public bool AddGameData(object gameDataObject)
        {
            if (typeof(RouletteBetData) == gameDataObject.GetType())
            {
                RouletteBets.Add((RouletteBetData)gameDataObject);
                return true;
            }
            else
                return true; 
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


    public class PlayerRandomQueueData
    {
        public List<String> PlayerQueue;
        public int currentQueueIndex;
        public string LastPlayerSpun;

        public bool shuffled;

        public PlayerRandomQueueData()
        {
            PlayerQueue = new List<string>();
            shuffled = false;
            currentQueueIndex = 0;
        }

        public string GetNextPlayerSpin(Random rnd, string playerName)
        {
            if (!shuffled)
            {
                ShuffleAllPlayers(rnd, false);
                shuffled = true;
            }

            string thisPlayer = PlayerQueue[currentQueueIndex];

            if (thisPlayer == playerName)
            {
                currentQueueIndex++;
                if (currentQueueIndex >= PlayerQueue.Count)
                {
                    currentQueueIndex = 0;
                }
                thisPlayer = PlayerQueue[currentQueueIndex];
            }

            currentQueueIndex++;
            if (currentQueueIndex >= PlayerQueue.Count)
            {
                ShuffleAllPlayers(rnd, true);
            }
            LastPlayerSpun = thisPlayer;
            return thisPlayer;
        }
        
        public void AddNewPlayer(Random rnd, string playerName)
        {
            int num = rnd.Next(0, PlayerQueue.Count);

            if (num < currentQueueIndex)
            {
                currentQueueIndex++;
            }

            PlayerQueue.Insert(num, playerName);
        }

        public void RemovePlayer(string playername)
        {
            int index = PlayerQueue.IndexOf(playername);
            if (index >= 0)
            {
                if (index < currentQueueIndex)
                {
                    currentQueueIndex -= 1;
                }
                PlayerQueue.RemoveAt(index);

                if (currentQueueIndex >= PlayerQueue.Count)
                {
                    Random rnd = new Random();
                    ShuffleAllPlayers(rnd, true);
                }
            }
        }

        public void ShuffleAllPlayers(Random rnd, bool moveLastTo2Plus)
        {
            currentQueueIndex = 0;
            if (PlayerQueue.Count >= 0)
            {
                List<string> newPlayerNames = new List<string>();
                int startingPlayerCount = PlayerQueue.Count;
                string movedPlayer = null;
                for (int i = 0; i < startingPlayerCount; i++)
                {
                    if (movedPlayer != null)
                    {
                        PlayerQueue.Add(movedPlayer);
                        movedPlayer = null;
                    }
                    if (i == 0 && moveLastTo2Plus && PlayerQueue.Count > 1)
                    {
                        movedPlayer = PlayerQueue[PlayerQueue.Count - 1];
                        PlayerQueue.RemoveAt(PlayerQueue.Count - 1);
                    }

                    int num = rnd.Next(0, PlayerQueue.Count);

                    string nextInQueue = PlayerQueue[num];
                    PlayerQueue.RemoveAt(num);
                    newPlayerNames.Add(nextInQueue);
                }
                PlayerQueue = newPlayerNames;
            }
        }
    }
}



