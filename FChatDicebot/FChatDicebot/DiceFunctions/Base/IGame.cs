using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.DiceFunctions
{
    public interface IGame
    {
        string GetGameName();
        int GetMaxPlayers();
        int GetMinPlayers();
        bool AllowAnte();
        bool KeepSessionDefault();

        string GetStartingDisplay();
        string GetEndingDisplay();

        string RunGame(System.Random r, List<String> playerNames, DiceBot diceBot, BotMain botMain, GameSession session); //run the game and get output

        string IssueGameCommand(DiceBot diceBot, BotMain botMain, string character, string channel, GameSession session, string[] terms);
    }
}
