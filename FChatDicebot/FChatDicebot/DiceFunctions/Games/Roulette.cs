using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.DiceFunctions
{
    public class Roulette : IGame
    {
        public string GetGameName()
        {
            return "Roulette";
        }

        public int GetMaxPlayers()
        {
            return 12;
        }

        public int GetMinPlayers()
        {
            return 1;
        }

        public bool AllowAnte()
        {
            return true;
        }

        public bool KeepSessionDefault()
        {
            return false;
        }

        public string GetStartingDisplay()
        {
            return "[eicon]dbroulette1[/eicon][eicon]dbroulette2[/eicon]";
        }

        public string GetEndingDisplay()
        {
            return "";
        }

        public string RunGame(System.Random r, List<String> playerNames, DiceBot diceBot, BotMain botMain, GameSession session)
        {
            int randomResult = r.Next(37);//0-36

            bool colorGreen = false;
            bool colorBlack = false;
            bool colorRed = false;

            List<int> redResults = new List<int>(){1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36};
            List<int> blackResults = new List<int>(){2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35};
            List<int> greenResults = new List<int>(){0};
            if(redResults.Contains(randomResult) )
                colorRed = true;
            if(blackResults.Contains(randomResult) )
                colorBlack = true;
            if(greenResults.Contains(randomResult) )
                colorGreen = true;

            string rollResult = "[color=gray]" + randomResult + " (black)[/color]";
            if(colorRed)
                rollResult = "[color=red]" + randomResult + " (red)[/color]";
            if (colorGreen)
                rollResult = "[color=green]" + randomResult + " (green)[/color]";

            string rouletteRollString = "[color=yellow]The dealer tosses in the ball! The wheel is spinning...[/color]\n[color=yellow]...[/color]\nThe ball landed on " + rollResult + "!";

            string characterBetsString = "";

            foreach(RouletteBetData bet in session.RouletteBets)
            {
                if (!playerNames.Contains(bet.characterName))
                {
                    characterBetsString += bet.characterName + " not found.\n";
                    continue;
                }

                if (!string.IsNullOrEmpty(characterBetsString))
                {
                    characterBetsString += "\n";
                }

                string betstring = "";
                
                ChipPile pile = diceBot.GetChipPile(bet.characterName, session.ChannelId, false);

                if(pile.Chips >= bet.amount)
                {
                    bet.cannotAffordBet = false;
                    betstring = diceBot.BetChips(bet.characterName, session.ChannelId, bet.amount, false);
                    betstring += " Their money is on [b]" + Utils.GetRouletteBetString(bet.bet, bet.specificNumberBet) + "[/b]!";

                }
                else
                {
                    bet.cannotAffordBet = true;
                    betstring = Utils.GetCharacterUserTags(bet.characterName) + " can no longer afford their bet.";
                }

                characterBetsString += betstring;
            }

            ChipPile houseChipsPile = diceBot.GetChipPile(DiceBot.HouseName, session.ChannelId, true);
            string claimPotString = diceBot.ClaimPot(DiceBot.HouseName, session.ChannelId, false, false);

            string betReturns = "";

            foreach (RouletteBetData bet in session.RouletteBets)
            {
                if (!playerNames.Contains(bet.characterName) || bet.cannotAffordBet)
                {
                    continue;
                }

                int betReturn = RouletteBetReturn(randomResult, redResults, blackResults, bet.bet, bet.specificNumberBet);

                string winningsString = Utils.GetCharacterUserTags(bet.characterName) + " has [b]" + (betReturn > 0 ? "won" : "lost") + "![/b] ";

                if(betReturn > 0)
                {
                    int betWonAmount = bet.amount * betReturn;

                    if (houseChipsPile.Chips < betWonAmount)
                    {
                        diceBot.AddChips(DiceBot.HouseName, session.ChannelId, betWonAmount, false);
                    }

                    winningsString += diceBot.GiveChips(DiceBot.HouseName, bet.characterName, session.ChannelId, betWonAmount, false);
                }

                if (!string.IsNullOrEmpty(betReturns))
                {
                    betReturns += "\n";
                }

                betReturns += winningsString;
            }

            session.State = DiceFunctions.GameState.Finished;

            string outputString = characterBetsString + "\n" + claimPotString + "\n" + rouletteRollString + "\n" + betReturns;
            
            return outputString;
        }

        public int RouletteBetReturn(int rouletteBallResult, List<int> redNumbers, List<int> blackNumbers, RouletteBet betMade, int specificNumberBet)
        {
            switch(betMade)
            {
                case RouletteBet.NONE:
                    return 0;
                case RouletteBet.Red:
                    if (redNumbers.Contains(rouletteBallResult))
                        return 2;
                    else
                        return 0;
                case RouletteBet.Black:
                    if (blackNumbers.Contains(rouletteBallResult))
                        return 2;
                    else
                        return 0;
                case RouletteBet.Even:
                    if (rouletteBallResult % 2 == 0 && rouletteBallResult != 0)
                    {
                        return 2;
                    }
                    else
                        return 0;
                case RouletteBet.Odd:
                    if (rouletteBallResult % 2 == 1 && rouletteBallResult != 0)
                    {
                        return 2;
                    }
                    else
                        return 0;
                case RouletteBet.OneToEighteen:
                    if (rouletteBallResult >= 1 && rouletteBallResult <= 18)
                    {
                        return 2;
                    }
                    else
                        return 0;
                case RouletteBet.NineteenToThirtySix:
                    if (rouletteBallResult >= 19 && rouletteBallResult <= 36)
                    {
                        return 2;
                    }
                    else
                        return 0;
                case RouletteBet.First12:
                    if (rouletteBallResult >= 1 && rouletteBallResult <= 12)
                    {
                        return 3;
                    }
                    else
                        return 0;
                case RouletteBet.Second12:
                    if (rouletteBallResult >= 13 && rouletteBallResult <= 24)
                    {
                        return 3;
                    }
                    else
                        return 0;
                case RouletteBet.Third12:
                    if (rouletteBallResult >= 25 && rouletteBallResult <= 36)
                    {
                        return 3;
                    }
                    else
                        return 0;
                case RouletteBet.SpecificNumber:
                    if (rouletteBallResult == specificNumberBet)
                    {
                        return 36;
                    }
                    else
                        return 0;
            }

            return 0;
        }

        public string IssueGameCommand(DiceBot diceBot, BotMain botMain, string character, string channel, GameSession session, string[] terms)
        {
            return GetGameName() + " has no valid GameCommands";
        }
    }

    public class RouletteBetData
    {
        public string characterName;
        public int amount;
        public int specificNumberBet;
        public RouletteBet bet;
        public bool cannotAffordBet;

        public string GetBetString()
        {
            return Utils.GetCharacterUserTags(characterName) + ": " + amount + " on " + Utils.GetRouletteBetString(bet, specificNumberBet);
        }
    }

    public enum RouletteBet
    {
        NONE,
        Red,
        Black,
        OneToEighteen,
        NineteenToThirtySix,
        First12,
        Second12,
        Third12,
        Even,
        Odd,
        SpecificNumber
    }
}
