using FChatDicebot.BotCommands.Base;
using FChatDicebot.SavedData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FChatDicebot.DiceFunctions
{
    public class DiceBot
    {
        private BotMain botMain;
        public System.Random random;
        public const int MaximumDice = 100;
        public const int MaximumRolls = 200;
        public const int MaximumSides = 100000;
        public const int MaximumCommandLength = 1000;
        public const int MaximumHandSize = 100;
        public const int MaximumDrawSize = 10;

        public static List<char> ValidOperators = new List<char>(){ '+', '-', '*', '/', '>', '<'};// '►', '◄' };

        public const string DealerName = "the_dealer";
        public const string BurnCardsName = "burn_cards";
        public const string DiscardName = "discard_pile";
        public const string HouseName = "the_house";

        public const string PotName = "the_pot";

        public const string PlaySuffix = "_inplay";

        public List<Deck> ChannelDecks;
        public List<Hand> Hands;
        public List<ChipPile> ChipPiles;
        public List<GameSession> GameSessions;
        public List<CountdownTimer> CountdownTimers;

        public List<IGame> PossibleGames;

        public DiceBot(BotMain sourceBot)
        {
            botMain = sourceBot;
            random = new System.Random();

            ChannelDecks = new List<Deck>();
            Hands = new List<Hand>();
            ChipPiles = new List<ChipPile>();
            GameSessions = new List<GameSession>();
            CountdownTimers = new List<CountdownTimer>();

            PossibleGames = new List<IGame>() { new HighRoll(), new TexasHoldem(), new BottleSpin(), new Roulette(), new KingsGame() };

            LoadChipPilesFromDisk(BotMain.FileFolder, BotMain.SavedChipsFileName);
        }

        private void LoadChipPilesFromDisk(string fileFolder, string fileName)
        {
            string path = Utils.GetTotalFileName(fileFolder, fileName);
            try
            {
                if (!Directory.Exists(fileFolder))
                {
                    Directory.CreateDirectory(fileFolder);
                }

                if (File.Exists(path))
                {
                    string fileText = File.ReadAllText(path, Encoding.ASCII);

                    ChipPiles = JsonConvert.DeserializeObject<List<ChipPile>>(fileText);

                    if (BotMain._debug)
                        Console.WriteLine("loaded LoadChipPiles successfully.");
                }
                else
                {
                    ChipPiles = new List<ChipPile>();
                    Console.WriteLine("LoadChipPiles file does not exist.");
                }
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("Exception: Failed to load LoadChipPiles for " + path + "\n" + exc.ToString());
            }
        }

        public string RollFitD(int diceNumber)
        {
            int actualRolled = diceNumber <= 0? 2 : diceNumber;
            DiceRoll roll = new DiceRoll()
            {
                DiceRolled = actualRolled,
                DiceSides = 6,
                TextFormat = true
            };
            roll.Roll(random);

            int success = roll.Rolls.Count(a => a == 4 || a == 5);
            int critical = roll.Rolls.Count(a => a == 6);
            int fail = roll.Rolls.Count(a => a < 4);

            if(diceNumber <= 0) //with 0 or less dice, you roll 2 and take the lowest without the possibility of critical success
            {
                success += critical;
                critical = 0;
                if (fail > 0)
                {
                    fail = 1;
                    success = 0;
                }
                else
                {
                    success = 1;
                }
            }

            string failString = "";
            if(fail > 0)
                failString = " fail: [color=red]" + fail + "[/color]";
            string successString = "";
            if(success > 0)
                successString = " success: [color=yellow]" + success + "[/color]";
            string criticalString = "";
            if(critical > 0)
                criticalString = " critical: [color=green]" + critical + "[/color]";

            string resultString = string.Format("{0} \nFitD [b]{1}[/b] dice, {2}{3}{4}", roll.ResultString(), diceNumber, failString, successString, criticalString);

            return resultString;
        }

        public string GetRollResult(string[] inputCommands, bool debugOutput)
        {
            string allCommands = Utils.CombineStringArray(inputCommands);

            if(allCommands.Length > MaximumCommandLength)
                return ((new DiceRoll() { Error = true, ErrorString = "command exceeds maximum length" }).ResultString());
            
            List<char> operators = new List<char>();
            string errorString = "";
            List<string> commandTerms = SeparateCommands(allCommands, out operators, out errorString);

            if(debugOutput)
                Console.WriteLine("Commands Separated: " + Utils.PrintList(commandTerms.ToArray()) + " operators found: " + Utils.PrintList(operators.ToArray()) );

            if (errorString != "")
                return ((new DiceRoll() { Error = true, ErrorString = errorString }).ResultString());
            if (operators == null || commandTerms.Count < 1)
                return ( (new DiceRoll() { Error = true, ErrorString = "requires at least 1 input" }).ResultString());
            if (operators.Count != commandTerms.Count - 1)
                return ((new DiceRoll() { Error = true, ErrorString = "bad number of arithmetic operators" }).ResultString());
            if (operators.Count(a => a == '<' || a == '>') > 1)
                return ((new DiceRoll() { Error = true, ErrorString = "only one comparison (< or >) allowed" }).ResultString());

            string totalResultString = "";
            List<DiceRoll> finishedRolls = new List<DiceRoll>();

            foreach(string s in commandTerms)
            {
                DiceRoll d = ParseRollFromCommand(s);

                d.Roll(random);

                finishedRolls.Add(d);

                if (totalResultString != "")
                    totalResultString += ", ";
                totalResultString += d.ResultString();
            }

            try
            {
                if (commandTerms.Count > 1 && operators.Count > 0)
                {
                    totalResultString += "\n[i]Result: [/i]";

                    List<int> allNumbers = new List<int>();
                    foreach (DiceRoll d2 in finishedRolls)
                    {
                        allNumbers.Add(d2.Total);
                    }

                    //put number results in arithmetic string
                    for(int i = 0; i < allNumbers.Count; i++)
                    {
                        if (i > 0)
                        {
                            totalResultString += operators[i - 1] + " ";
                        }
                        totalResultString += allNumbers[i] + " ";
                    }

                    //OOP first */
                    string applyOperatorErrorString = "";
                    ApplyOperator(ref allNumbers, ref operators, '*', out applyOperatorErrorString);
                    if(applyOperatorErrorString != "")
                        return ((new DiceRoll() { Error = true, ErrorString = applyOperatorErrorString }).ResultString());

                    ApplyOperator(ref allNumbers, ref operators, '/', out applyOperatorErrorString);
                    if (applyOperatorErrorString != "")
                        return ((new DiceRoll() { Error = true, ErrorString = applyOperatorErrorString }).ResultString());
                    
                    //OOP second +-
                    ApplyOperator(ref allNumbers, ref operators, '+', out applyOperatorErrorString);
                    if (applyOperatorErrorString != "")
                        return ((new DiceRoll() { Error = true, ErrorString = applyOperatorErrorString }).ResultString());

                    ApplyOperator(ref allNumbers, ref operators, '-', out applyOperatorErrorString);
                    if (applyOperatorErrorString != "")
                        return ((new DiceRoll() { Error = true, ErrorString = applyOperatorErrorString }).ResultString());
                    
                    //OOP comparison < > 
                    if (operators.Contains('>') || operators.Contains('<'))
                    {
                        string additionalStr = "";
                        if (allNumbers.Count == 2 && operators.Contains('>'))
                        {
                            additionalStr = allNumbers[0] > allNumbers[1] ? "true":  "false";
                        }
                        else if (allNumbers.Count == 2 && operators.Contains('<'))
                        {
                            additionalStr = allNumbers[0] < allNumbers[1] ? "true" : "false";
                        }
                        else
                        {
                            return ((new DiceRoll() { Error = true, ErrorString = "invalid comparison" }).ResultString());
                        }
                        totalResultString += "= [b]" + additionalStr + "[/b]";
                    }
                    else
                    {
                        totalResultString += "= [b]" + allNumbers[0] + "[/b]";
                    }

                }
            }catch(Exception exc)
            {
                return ((new DiceRoll() { Error = true, ErrorString = "exception on arithmetic: " + exc.ToString() }).ResultString());
            }

            return totalResultString;
        }

        public void ApplyOperator(ref List<int> allNumbers, ref List<char> operators, char op, out string error)
        {
            error = "";

            while (operators.Contains(op))
            {
                int thisOperatorIndex = operators.IndexOf(op);
                int result = 0;
                double resultcheck = 0;
                switch(op)
                {
                    case '*':
                        result = allNumbers[thisOperatorIndex] * allNumbers[thisOperatorIndex + 1];
                        resultcheck = (double) allNumbers[thisOperatorIndex] * (double) allNumbers[thisOperatorIndex + 1];
                        break;
                    case '/':
                        result = allNumbers[thisOperatorIndex] / allNumbers[thisOperatorIndex + 1];
                        resultcheck = result;
                        break;
                    case '+':
                        result = allNumbers[thisOperatorIndex] + allNumbers[thisOperatorIndex + 1];
                        resultcheck = (double)allNumbers[thisOperatorIndex] + (double)allNumbers[thisOperatorIndex + 1];
                        break;
                    case '-':
                        result = allNumbers[thisOperatorIndex] - allNumbers[thisOperatorIndex + 1];
                        resultcheck = (double)allNumbers[thisOperatorIndex] - (double)allNumbers[thisOperatorIndex + 1];
                        break;
                }

                if ((double)result != resultcheck)
                {
                    error = "integer overflow";
                    return;
                }

                allNumbers.RemoveAt(thisOperatorIndex + 1);
                allNumbers[thisOperatorIndex] = result;
                operators.RemoveAt(thisOperatorIndex);
            }
        }

        private List<string> SeparateCommands(string allCommands, out List<char> operators, out string error)
        {
            operators = new List<char>();
            error = "";

            List<string> commandTerms = new List<string>();

            if(string.IsNullOrEmpty(allCommands))
            {
                return commandTerms;
            }
            
            int index = 0;
            int lastOperatorIndex = -1;
            foreach(char c in allCommands)
            {
                if(ValidOperators.Contains(c))
                {
                    //check for negative
                    if (lastOperatorIndex == index -1)
                    {
                        if(c == '-')
                        {
                            if(allCommands.Length <= index)
                            {
                                error = "invalid - operator";
                                return commandTerms;
                            }
                        }
                        else
                        {
                            error = "operators must be separated by terms";
                            return commandTerms;
                        }
                    }
                    else
                    {
                        operators.Add(c);

                        commandTerms.Add(allCommands.Substring(lastOperatorIndex + 1, index - (lastOperatorIndex + 1)));

                        lastOperatorIndex = index;
                    }
                }

                index++;
            }
            //add last term without an operator after it
            if (lastOperatorIndex + 1 < allCommands.Length)
                commandTerms.Add(allCommands.Substring(lastOperatorIndex + 1, allCommands.Length - (lastOperatorIndex + 1)));

            return commandTerms;
        }

        public DiceRoll ParseRollFromCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return new DiceRoll() { Error = true, ErrorString = "command empty" };
            
            //try parsing as a plain number if there is no dice roll
            if(!command.Contains('d'))
            {
                int numberResult = 0;
                bool successNumber = int.TryParse(command, out numberResult);
                if (!successNumber)
                    return new DiceRoll() { Error = true, ErrorString = "invalid number input" };
                else
                    return new DiceRoll() { Error = false, TextFormat = true, Total = numberResult };
            }

            if (command.Length < 3 || !command.Contains('d') || command.LastIndexOf('d') != command.IndexOf('d')
                || command.IndexOf('d') == 0 || command.IndexOf('d') == command.Length -1)
                return new DiceRoll() { Error = true, ErrorString = "invalid dice input" };

            int numberSides = 0;
            int numberDice = 0;
            bool explode = false;

            string[] split = command.Split('d');
            string splitDiceNumber = split[0];
            string splitDiceSidesAndCommands = split[1];
            bool success1 = int.TryParse(splitDiceNumber, out numberDice);

            string parseError = "";

            //parse out '!'
            if (splitDiceSidesAndCommands.Contains('!'))
            {
                explode = true;
            }

            int explodeThreshold = ParseOutDieCommand("!", ref splitDiceSidesAndCommands, out parseError);
            //note: no error for ! on purpose: it is possible to parse out nothing for the number.
            splitDiceSidesAndCommands = splitDiceSidesAndCommands.Replace("!0", "").Replace("!", "");

            int keepHighest = ParseOutDieCommand("kh", ref splitDiceSidesAndCommands, out parseError);

            if (!string.IsNullOrEmpty(parseError))
                return new DiceRoll() { Error = true, ErrorString = parseError };

            int keepLowest = ParseOutDieCommand("kl", ref splitDiceSidesAndCommands, out parseError);

            if (!string.IsNullOrEmpty(parseError))
                return new DiceRoll() { Error = true, ErrorString = parseError };
            int removeHighest = ParseOutDieCommand("rh", ref splitDiceSidesAndCommands, out parseError);

            if (!string.IsNullOrEmpty(parseError))
                return new DiceRoll() { Error = true, ErrorString = parseError };

            int removeLowest = ParseOutDieCommand("rl", ref splitDiceSidesAndCommands, out parseError);

            if (!string.IsNullOrEmpty(parseError))
                return new DiceRoll() { Error = true, ErrorString = parseError };

            int rerollNumber = ParseOutDieCommand("rr", ref splitDiceSidesAndCommands, out parseError);

            if (!string.IsNullOrEmpty(parseError))
                return new DiceRoll() { Error = true, ErrorString = parseError };
            
            int countOver = ParseOutDieCommand("co", ref splitDiceSidesAndCommands, out parseError);

            if (!string.IsNullOrEmpty(parseError))
                return new DiceRoll() { Error = true, ErrorString = parseError };

            int countUnder = ParseOutDieCommand("cu", ref splitDiceSidesAndCommands, out parseError);

            if (!string.IsNullOrEmpty(parseError))
                return new DiceRoll() { Error = true, ErrorString = parseError };

            bool success2 = int.TryParse(splitDiceSidesAndCommands, out numberSides);


            if (numberDice > MaximumDice)
                return new DiceRoll() { Error = true, ErrorString = "will only roll up to " + MaximumDice + " base dice" };
            if (numberSides > MaximumSides)
                return new DiceRoll() { Error = true, ErrorString = "will only roll up to " + MaximumSides + " base sides" };
            if (numberSides == 1)
                return new DiceRoll() { Error = true, ErrorString = "1 is not a valid number of sides" };
            if (numberSides <= 0 || numberDice <= 0)
                return new DiceRoll() { Error = true, ErrorString = "number of sides and number of dice must be greater than 0" };
            if (!success1)
                return new DiceRoll() { Error = true, ErrorString = "invalid number of dice" };
            if (!success2)
                return new DiceRoll() { Error = true, ErrorString = "invalid number of sides" };
            if (explodeThreshold != 0 && explodeThreshold <= 1)// (int)(numberSides / 2) )
                return new DiceRoll() { Error = true, ErrorString = "cannot use an explode threshold of 1 or less."};// less than half the dice total" };
            if ((keepHighest > 0 && (keepLowest > 0 || removeHighest > 0 || removeLowest > 0)) ||
                (keepLowest > 0 && (keepHighest > 0 || removeHighest > 0 || removeLowest > 0)) ||
                (removeHighest > 0 && (keepHighest > 0 || keepLowest > 0 || removeLowest > 0)) ||
                (removeLowest > 0 && (keepHighest > 0 || removeHighest > 0 || keepLowest > 0))
                )
                return new DiceRoll() { Error = true, ErrorString = "only one keep/ drop option allowed per roll" };
            if ((keepHighest >= numberDice) ||
                (keepLowest >= numberDice) ||
                (removeHighest >= numberDice) ||
                (removeLowest >= numberDice)
                )
                return new DiceRoll() { Error = true, ErrorString = "number of dice to choose must be less than total number of dice" };


            return new DiceRoll()
            {
                DiceRolled = numberDice,
                DiceSides = numberSides,
                Error = false,
                Explode = explode,
                ExplodeThreshold = explodeThreshold,
                TextFormat = true,
                KeepHighest = keepHighest,
                KeepLowest = keepLowest,
                RemoveHighest = removeHighest,
                RemoveLowest = removeLowest,
                RerollNumber = rerollNumber,
                CountOver = countOver,
                CountUnder = countUnder
            };
        }

        private int ParseOutDieCommand(string currentParseCommand, ref string splitDiceSidesAndCommands, out string errorString)
        {
            int resultNumber = 0;
            errorString = null;

            if (splitDiceSidesAndCommands.Contains(currentParseCommand))
            {
                string error = "";
                string remainingString = "";
                resultNumber = keepNumberParse(splitDiceSidesAndCommands, currentParseCommand, out error, out remainingString);

                if (error != "")
                {
                    errorString = error;
                    return resultNumber;
                }

                if (resultNumber > 0)
                {
                    splitDiceSidesAndCommands = remainingString;
                }
            }

            return resultNumber;
        }

        public int keepNumberParse(string parseString, string keepCharacters, out string errorString, out string remainingString)
        {
            errorString = "";
            remainingString = "";

            int indexOfParseString = parseString.IndexOf(keepCharacters);

            if (indexOfParseString == -1)
                return 0;

            if (parseString.Length <= indexOfParseString + keepCharacters.Length || !char.IsDigit(parseString[indexOfParseString + keepCharacters.Length]))
            {
                errorString = "'" + keepCharacters + "' must be followed by a number";
                return 0;
            }

            string khNumber = "";

            string tempString = parseString.Substring(indexOfParseString + keepCharacters.Length);

            while(tempString.Length > 0 && char.IsDigit(tempString[0]))
            {
                khNumber += tempString[0];
                tempString = tempString.Substring(1);
            }

            remainingString = parseString.Replace(keepCharacters + khNumber, "");

            int returnInt = 0;
            bool parsed = int.TryParse(khNumber, out returnInt);

            if (!parsed)
            {
                errorString = "failed to parse " + keepCharacters + " number";
                return 0;
            }
            if (parsed && returnInt <= 0)
            {
                errorString = keepCharacters + " cannot be 0 or less dice";
                return 0;
            }

            return returnInt;
        }
        
        public string DrawCards(int numberDraws, bool includeJoker, bool fromDeck, string channelId, DeckType deckType, string character, bool secretDraw, out string trueDraw)
        {
            if (numberDraws <= 0)
                numberDraws = 1;

            if (numberDraws > MaximumDrawSize)
                numberDraws = MaximumDrawSize;

            string totalDrawString = "";
            trueDraw = "";

            Hand thisCharacterHand = GetHand(channelId, deckType, character);

            int actualDraws = 0;

            for(int i = 0; i < numberDraws; i++)
            {
                if (!string.IsNullOrEmpty(totalDrawString))
                    totalDrawString += ", ";

                if(thisCharacterHand.CardsCount() >= MaximumHandSize)
                {
                    totalDrawString += "(maximum hand size reached!)";
                    break;
                }

                string customDeckName = Utils.GetCustomDeckName(character);
                DeckCard d = DrawCard(random, includeJoker, fromDeck, channelId, deckType, customDeckName);
                actualDraws++;

                if(d!= null)
                {
                    thisCharacterHand.AddCard(d);

                    totalDrawString += d.ToString();
                }
                else
                {
                    totalDrawString += "(out of cards)";
                    break;
                }
            }

            trueDraw = totalDrawString + "";
            if(secretDraw)
            {
                string cardsS = "";
                if (actualDraws > 1 || actualDraws == 0)
                    cardsS = "s";

                totalDrawString = actualDraws + " card" + cardsS;
            }

            //true draw is what is drawn, rather than the output (which might hide draws).
            trueDraw += "\n[i]Current Hand: [/i]" + thisCharacterHand.ToString();
            totalDrawString += "\n[i]Current Hand: [/i]" + thisCharacterHand.ToString(secretDraw);

            return totalDrawString;
        }

        public string DiscardCards(List<int> discardsList, bool all, string channelId, DeckType deckType, string character, out int actualDiscards)
        {
            return MoveCards(CardMoveType.DiscardCard, discardsList, all, channelId, deckType, character, out actualDiscards);   
        }

        public string DiscardCardsFromPlay(List<int> discardsList, bool all, string channelId, DeckType deckType, string character, out int actualDiscards)
        {
            return MoveCards(CardMoveType.ToDiscardFromPlay, discardsList, all, channelId, deckType, character, out actualDiscards);
        }

        public string PlayCards(List<int> playCardsList, bool all, string channelId, DeckType deckType, string character, out int actualPlayedCount)
        {
            return MoveCards(CardMoveType.PlayCard, playCardsList, all, channelId, deckType, character, out actualPlayedCount);   
        }

        public string PlayCardsFromDiscard(List<int> playCardsList, bool all, string channelId, DeckType deckType, string character, out int actualPlayedCount)
        {
            return MoveCards(CardMoveType.ToPlayFromDiscard, playCardsList, all, channelId, deckType, character, out actualPlayedCount);
        }

        private string MoveCards(CardMoveType cardMoveType, List<int> moveCardsList, bool all, string channelId, DeckType deckType, string character, out int actualMovedCount)
        {
            actualMovedCount = 0;

            Hand moveFromPile = null;
            Hand moveToPile = null;
            switch(cardMoveType)
            {
                case CardMoveType.DiscardCard:
                    {
                        moveFromPile = GetHand(channelId, deckType, character);
                        moveToPile = GetHand(channelId, deckType, DiscardName);
                    }
                    break;
                case CardMoveType.PlayCard:
                    {
                        moveFromPile = GetHand(channelId, deckType, character);
                        moveToPile = GetHand(channelId, deckType, character + PlaySuffix);
                    }
                    break;
                case CardMoveType.ToHandFromDiscard:
                    {
                        moveFromPile = GetHand(channelId, deckType, DiscardName);
                        moveToPile = GetHand(channelId, deckType, character);
                    }
                    break;
                case CardMoveType.ToPlayFromDiscard:
                    {
                        moveFromPile = GetHand(channelId, deckType, DiscardName);
                        moveToPile = GetHand(channelId, deckType, character + PlaySuffix);
                    }
                    break;
                case CardMoveType.ToDiscardFromPlay:
                    {
                        moveFromPile = GetHand(channelId, deckType, character + PlaySuffix);
                        moveToPile = GetHand(channelId, deckType, DiscardName);
                    }
                    break;
                case CardMoveType.ToHandFromPlay:
                    {
                        moveFromPile = GetHand(channelId, deckType, character + PlaySuffix);
                        moveToPile = GetHand(channelId, deckType, character);
                    }
                    break;
            }

            if(moveToPile == null)
            {
                return "(Error: moveto pile not found)";
            }
            else if (moveToPile == null)
            {
                return "(Error: movefrom pile not found)";
            }

            if (moveFromPile.Empty())
            {
                if(cardMoveType == CardMoveType.DiscardCard || cardMoveType == CardMoveType.PlayCard )
                {
                    return "(hand was empty)";
                }
                else if(cardMoveType == CardMoveType.ToDiscardFromPlay || cardMoveType == CardMoveType.ToHandFromPlay)
                {
                    return "(no cards in play)";
                }
                else if (cardMoveType == CardMoveType.ToHandFromDiscard || cardMoveType == CardMoveType.ToPlayFromDiscard)
                {
                    return "(no cards in discard)";
                }
                else
                {
                    return "(no cards in origin pile)";
                }
            }

            if (all)
            {
                moveCardsList = new List<int>();
                for (int i = 0; i < moveFromPile.CardsCount(); i++)
                {
                    moveCardsList.Add(i);
                }
            }

            if (moveCardsList == null || moveCardsList.Count <= 0)
                moveCardsList = new List<int>() { 0 };

            if (moveCardsList.Count > MaximumDrawSize)
                moveCardsList = moveCardsList.Take(MaximumDrawSize).ToList();

            string totalOutputString = "";

            bool showHand = true;

            int currentHandSize = moveFromPile.CardsCount();

            List<DeckCard> newHandCards = new List<DeckCard>();

            for (int i = 0; i < currentHandSize; i++)
            {
                if (moveCardsList.Contains(i))
                {
                    if (!string.IsNullOrEmpty(totalOutputString))
                        totalOutputString += ", ";

                    DeckCard d = moveFromPile.GetCardAtIndex(i);
                    totalOutputString += d.ToString();
                    moveToPile.AddCard(d);
                }
                else
                {
                    newHandCards.Add(moveFromPile.GetCardAtIndex(i));
                }
            }

            moveFromPile.ResetHand();

            foreach (DeckCard d in newHandCards)
            {
                moveFromPile.AddCard(d);
            }

            actualMovedCount = currentHandSize - moveFromPile.CardsCount();

            if (showHand)
            {
                totalOutputString += "\n[i]Current " + moveFromPile.GetCollectionName() + ": [/i]" + moveFromPile.ToString(true);
                totalOutputString += "\n[i]Current " + moveToPile.GetCollectionName() + ": [/i]" + moveToPile.ToString(true);
            }

            return totalOutputString;
        }

        public string TakeCardsFromPlay(List<int> drawFromPlayList, bool all, string channelId, DeckType deckType, string character, out int movedCount)
        {
            return MoveCards(CardMoveType.ToHandFromPlay, drawFromPlayList, all, channelId, deckType, character, out movedCount);
        }

        public string TakeCardsFromDiscard(List<int> drawFromDiscardList, bool all, string channelId, DeckType deckType, string character, out int movedCount)
        {
            return MoveCards(CardMoveType.ToHandFromDiscard, drawFromDiscardList, all, channelId, deckType, character, out movedCount);

            //Hand thisCharacterHand = GetHand(channelId, deckType, character);
            //Hand discardHand = GetHand(channelId, deckType, DiscardName);

            //if (discardHand.Empty())
            //    return "(discard pile was empty)";

            ///////////////
            //if (all)
            //{
            //    drawFromDiscardList = new List<int>();
            //    for (int i = 0; i < discardHand.CardsCount(); i++)
            //    {
            //        drawFromDiscardList.Add(i);
            //    }
            //}

            //if (drawFromDiscardList == null || drawFromDiscardList.Count <= 0)
            //    drawFromDiscardList = new List<int>() { discardHand.CardsCount() - 1 };

            //if (drawFromDiscardList.Count > MaximumDrawSize)
            //    drawFromDiscardList = drawFromDiscardList.Take(MaximumDrawSize).ToList();

            //string totalDrawString = "";

            //bool showDiscard = false;
            //if (!discardHand.Empty())
            //    showDiscard = true;

            //int currentDiscardSize = discardHand.CardsCount();

            //List<DeckCard> newDiscardCards = new List<DeckCard>();

            //for (int i = 0; i < currentDiscardSize; i++)//
            //{
            //    if (drawFromDiscardList.Contains(i))
            //    {
            //        if (!string.IsNullOrEmpty(totalDrawString))
            //            totalDrawString += ", ";

            //        DeckCard d = discardHand.GetCardAtIndex(i);
            //        thisCharacterHand.AddCard(d);
            //        totalDrawString += d.ToString();
            //    }
            //    else
            //    {
            //        newDiscardCards.Add(discardHand.GetCardAtIndex(i));
            //    }
            //}

            //discardHand.ResetHand();

            //foreach (DeckCard d in newDiscardCards)
            //{
            //    discardHand.AddCard(d);
            //}

            //if (showDiscard)
            //{
            //    totalDrawString += "\n[i]Current Discard Pile: [/i]" + discardHand.ToString();
            //}

            //return totalDrawString;
        }

        public string GetRollTableResult(List<SavedRollTable> savedTables, string tableName, string character, string channel, int rollModifier, bool includeLabel, bool includeSecondaryRolls, int callDepth)
        {
            SavedRollTable savedTable = Utils.GetTableFromId(savedTables, tableName);
            if (savedTable == null || savedTable.Table == null)
            {
                return "Table " + tableName + " not found.";
            }

            RollTable table = savedTable.Table;

            if(table != null)
            {
                TableRollResult res = table.GetRollResult(random, rollModifier, includeLabel, includeSecondaryRolls);

                string returnString = res.ResultString;
                string addition = "";

                if(res.TriggeredRolls != null && res.TriggeredRolls.Count > 0 && callDepth > 0)
                {
                    if(callDepth > 0)
                    {
                        foreach (TableRollTrigger trt in res.TriggeredRolls)
                        {
                            if(trt.TableId.StartsWith("!"))
                            {
                                if(trt.TableId.StartsWith("!roll"))
                                {
                                    string commandName = "";
                                    string[] commandTerms = botMain.SeparateCommandTerms(trt.TableId, out commandName);

                                    string rollResult = GetRollResult(commandTerms, false);

                                    addition += "\n" + rollResult;
                                }
                                else if(trt.TableId.StartsWith("!drawcard"))
                                {
                                    string commandName = "";
                                    string[] rawTerms = botMain.SeparateCommandTerms(trt.TableId, out commandName);

                                    string[] terms = Utils.LowercaseStrings(rawTerms);

                                    CardCommandOptions options = new CardCommandOptions(botMain.BotCommandController, terms, character);

                                    int numberDrawn = Utils.GetNumberFromInputs(terms);
                                    if (numberDrawn > 1)
                                        options.cardsS = "s";

                                    string customDeckName = Utils.GetCustomDeckName(character);
                                    string deckTypeString = Utils.GetDeckTypeStringHidePlaying(options.deckType, customDeckName);
                                    string cardDrawingCharacterString = Utils.GetCharacterStringFromSpecialName(options.characterDrawName);

                                    string trueDraw = "";
                                    string messageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + options.cardsS + " drawn:[/i] " + DrawCards(numberDrawn, options.jokers, options.deckDraw, channel, options.deckType, options.characterDrawName, options.secretDraw, out trueDraw);

                                    if (options.secretDraw && !(options.characterDrawName == DiceBot.DealerName || options.characterDrawName == DiceBot.BurnCardsName || options.characterDrawName == DiceBot.DiscardName))
                                    {
                                        string playerMessageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + options.cardsS + " drawn:[/i] " + trueDraw;
                                        botMain.SendPrivateMessage(playerMessageOutput, character);
                                    }

                                    addition += "\n" + messageOutput;
                                }
                            }
                            else
                            {
                                addition += "\n" + GetRollTableResult(savedTables, trt.TableId.ToLower(), character, channel, trt.RollBonus, includeLabel, includeSecondaryRolls, callDepth - 1);
                            }

                        }
                    }
                    else
                    {
                        addition = "\n(maximum call depth exceeded!)";
                    }
                }

                return returnString + addition;
            }
            else
            {
                return "(savedTable inner table not found for " + tableName + ")";
            }
        }

        public DeckCard DrawCard(System.Random random, bool includeJoker, bool fromDeck, string channelId, DeckType deckType, string customDeckName)
        {
            DeckCard rtnCard = new DeckCard();

            if (fromDeck)
            {
                Deck thisDeck = GetDeck(channelId, deckType, customDeckName);
                if (thisDeck != null)
                {
                    DeckCard drawn = thisDeck.DrawCard();

                    if (drawn != null)
                        drawn.cardState = GetRandomCardStateForDraw(random, thisDeck.DeckType);

                    rtnCard = drawn;
                }
                else
                    return null;
            }
            else
            {
                int suite = random.Next(4);
                int number = random.Next(13) + 1;
                int joker = random.Next(54);

                rtnCard.suit = suite;
                rtnCard.number = number;
                rtnCard.joker = (joker < 2 && includeJoker);
            }

            return rtnCard;
        }

        public string GetRandomCardStateForDraw(System.Random rnd, DeckType deckType)
        {
            string cardState = null;
            switch (deckType)
            {
                case DiceFunctions.DeckType.Playing:
                case DiceFunctions.DeckType.ManyThings:
                    break;
                case DiceFunctions.DeckType.Tarot:
                    if (Utils.Percentile(rnd, 50))
                        cardState = "↑";// "Upright";
                    else
                        cardState = "↓";// "Inverted"; //TODO: add option for these in word form instead of symbol by channel setting
                    break;
            }

            return cardState;
        }

        public void ShuffleDeck(Random r, string channelId, DeckType deckType, bool fullShuffle, string customDeckName)
        {
            Deck relevantDeck = GetDeck(channelId, deckType, customDeckName);

            if (relevantDeck != null)
            {
                if (fullShuffle)
                {
                    EndHand(channelId, deckType);
                    relevantDeck.ShuffleFullDeck(r);
                }
                else
                    relevantDeck.ShuffleRemainingDeck(r);
            }
        }

        public void ResetDeck(bool jokers, string channelId, DeckType deckType, string customDeckName)
        {
            Deck relevantDeck = GetDeck(channelId, deckType, customDeckName);

            SavedDeck d = Utils.GetDeckFromId(botMain.SavedDecks, customDeckName);
            if(relevantDeck != null && d == null)
            {
                RemoveDeck(relevantDeck);
            }
            else if(relevantDeck != null)
            {
                EndHand(channelId, deckType);

                relevantDeck.FillDeck(jokers, d);

                relevantDeck.ResetCardStates();
                relevantDeck.ShuffleFullDeck(random);
            }
        }

        public Deck GetDeck(string channelId, DeckType deckType, string customDeckName)
        {
            string deckKey = GetDeckKey(channelId, deckType);
            Deck thisDeck = ChannelDecks.FirstOrDefault(a => a.Id == deckKey);
            if (thisDeck == null)
            {
                thisDeck = new Deck(deckType);
                thisDeck.Id = deckKey;

                if(deckType == DeckType.Custom)
                {
                    SavedDeck d = Utils.GetDeckFromId(botMain.SavedDecks, customDeckName);
                    if (d == null)
                        return null;

                    thisDeck.FillDeck(false, d);
                }
                else
                {
                    thisDeck.FillDeck(false);
                }

                thisDeck.ShuffleFullDeck(random);
                ChannelDecks.Add(thisDeck);
            }

            return thisDeck;
        }

        public void RemoveDeck(Deck removal)
        {
            ChannelDecks.Remove(removal);

        }

        public void EndHand(string channelId, DeckType deckType)
        {
            List<Hand> thisChannelHands = GetChannelHands(channelId, deckType);

            foreach(Hand h in thisChannelHands)
            {
                h.ResetHand();
            }
        }

        public Hand GetHand(string channelId, DeckType deckType, string character)
        {
            string deckKey = GetDeckKey(channelId, deckType);
            Hand h = Hands.FirstOrDefault(a => a.Id == deckKey && a.Character == character);
            if (h == null)
            {
                h = new Hand();
                h.Id = deckKey;
                h.Character = character;

                if (character == DiscardName)
                    h.SetCollectionName(BotMain.DiscardCollectionName);
                if (character.Contains(PlaySuffix))
                    h.SetCollectionName(BotMain.InPlayCollectionName);

                Hands.Add(h);
            }

            return h;
        }

        public List<Hand> GetChannelHands(string channelId, DeckType deckType)
        {
            string deckKey = GetDeckKey(channelId, deckType);
            return Hands.Where(a => a.Id == deckKey).ToList();
        }

        private List<ChipPile> GetChannelChipPiles(string channelId)
        {
            return ChipPiles.Where(a => a.ChannelId == channelId).ToList();
        }

        public string ListAllChipPiles(string channelName)
        {
            string rtnString = "";
            List<ChipPile> channelPiles = GetChannelChipPiles(channelName);

            if(channelPiles.Count == 0)
            {
                rtnString = "No piles found.";
            }
            else
            {
                foreach(ChipPile p in channelPiles)
                {
                    if(!SpecialCharacterName(p.Character))
                    {
                        if (!string.IsNullOrEmpty(rtnString))
                            rtnString += ", ";
                        string charString = GetCharacterNameString(p.Character);
                        rtnString += charString + ": " + p.Chips;
                    }
                }
            }

            return rtnString;
        }

        public string AddChips(string characterName, string channelId, int amount, bool pot)
        {
            ChipPile characterPile = GetChipPile(characterName, channelId);
            ChipPile potPile = GetChipPile(PotName, channelId);
            string characterNameUsed = Utils.GetCharacterUserTags(characterName);
            if(pot)
            {
                potPile.Chips += amount;
                characterNameUsed = "[b]the pot[/b]";

                if (potPile.Chips < 0)
                    potPile.Chips = 0;
            }
            else
            {
                characterPile.Chips += amount;

                if (characterPile.Chips < 0)
                    characterPile.Chips = 0;
            }

            string addedAmount = amount + " chips were added to ";
            if (amount < 0)
                addedAmount = (amount * -1) + " chips were removed from ";

            return addedAmount + characterNameUsed + "'s pile.";
        }

        public string RemoveChipsPile(string characterName, string channelId)
        {
            ChipPile characterPile = GetChipPile(characterName, channelId);
            int amt = characterPile.Chips;
            ChipPiles.RemoveAll(a => a.Character == characterName && a.ChannelId == channelId);

            return Utils.GetCharacterUserTags( characterName) + " removed their pile of chips. (" + amt + ")";
        }

        public string RemoveAllChipsPiles(string characterName, string channelId)
        {
            string channelPiles = ListAllChipPiles(channelId);
            int pilesCount = ChipPiles.Count(a => a.ChannelId == channelId);
            ChipPiles.RemoveAll(a => a.ChannelId == channelId);

            return Utils.GetCharacterUserTags(characterName) + " removed all the channel piles of chips. (" + pilesCount + "): " + channelPiles;
        }

        public string BetChips(string characterName, string channelId, int amount, bool all)
        {
            ChipPile characterPile = GetChipPile(characterName, channelId);
            ChipPile potPile = GetChipPile(PotName, channelId);
            int moved = MoveChipsFromPile(characterPile, potPile, amount, all);

            string allInString = characterPile.Chips == 0 && moved > 0 ? " [color=red][b](ALL IN)[/b][/color]" : "";
            return Utils.GetCharacterUserTags(characterName) + " added " + moved + " chips to the pot." + allInString;
        }

        public string GiveChips(string characterName, string targetCharacterName, string channelId, int amount, bool all)
        {
            ChipPile characterPile = GetChipPile(characterName, channelId);
            ChipPile targetPile = GetChipPile(targetCharacterName, channelId, false);

            if (targetPile == null)
            {
                return Utils.GetCharacterUserTags(targetCharacterName) + " must have a chips pile (by typing [b]!register[/b]) before giving them chips.";
            }

            int moved = MoveChipsFromPile(characterPile, targetPile, amount, all);

            string characterString = SpecialCharacterName(characterName)? Utils.GetCharacterStringFromSpecialName(characterName) : Utils.GetCharacterUserTags(characterName);
            string allInString = characterPile.Chips == 0 && moved > 0 ? " [color=red][b](100%)[/b][/color]" : "";
            return characterString + " gave " + moved + " chips to " + Utils.GetCharacterUserTags(targetCharacterName) + " " + allInString;
        }

        public string TakeChips(string characterName, string targetCharacterName, string channelId, int amount, bool all)
        {
            ChipPile characterPile = GetChipPile(characterName, channelId);
            ChipPile targetPile = GetChipPile(targetCharacterName, channelId, false);

            if (targetPile == null)
            {
                return Utils.GetCharacterUserTags(targetCharacterName) + " must have a chips pile before taking their chips.";
            }

            int moved = MoveChipsFromPile(targetPile, characterPile, amount, all);

            string allInString = characterPile.Chips == 0 && moved > 0 ? " [color=red][b](100%)[/b][/color]" : "";
            return Utils.GetCharacterUserTags(characterName) + " took " + moved + " chips from " + Utils.GetCharacterUserTags(targetCharacterName) + " " + allInString;
        }

        public string ClaimPot(string characterName, string channelId, bool half, bool third)
        {
            ChipPile claimingPile = GetChipPile(characterName, channelId);
            ChipPile potPile = GetChipPile(PotName, channelId);

            int amount = third? potPile.Chips / 3 : ( half? potPile.Chips / 2 : potPile.Chips );

            int moved = MoveChipsFromPile(potPile, claimingPile, amount, false);
            string fractionString = half ? "half " : (third ? "a third of " : "");

            string charString = Utils.GetCharacterUserTags(characterName);
            if (SpecialCharacterName(characterName))
            {
                charString = Utils.GetCharacterStringFromSpecialName(characterName);
            }

            return charString + " claimed " + fractionString + "the pot ([b]" + moved + " chips[/b]).";
        }

        private int MoveChipsFromPile(ChipPile sourcePile, ChipPile destinationPile, int amount, bool all)
        {
            int creditedAmount = 0;
            if(amount >= sourcePile.Chips || all)
            {
                creditedAmount = sourcePile.Chips;
                sourcePile.Chips = 0;
            }
            else
            {
                sourcePile.Chips -= amount;
                creditedAmount = amount;
            }

            destinationPile.Chips += creditedAmount;
            return creditedAmount;
        }

        public string DisplayChipPile(string channelName, string character)
        {
            ChipPile characterPile = GetChipPile(character, channelName);

            string charString = GetCharacterNameString(character);

            return charString + " chips pile: [b]" + characterPile.Chips + "[/b]";
        }

        public ChipPile GetChipPile(string characterName, string channelId, bool createNew = true)
        {
            ChipPile h = ChipPiles.FirstOrDefault(a => a.ChannelId == channelId && a.Character.ToLower() == characterName.ToLower());

            if (h == null && createNew)
            {
                ChannelSettings thisChannelSettings = botMain.GetChannelSettings(channelId);
                bool specialAccount = SpecialCharacterName(characterName);
                int startingChipsAmount = thisChannelSettings.StartWith500Chips ? BotMain.StartingChipsInPile : 0;

                h = new ChipPile();
                h.ChannelId = channelId;
                h.Character = characterName;
                h.Chips = specialAccount? 0 : startingChipsAmount;
                ChipPiles.Add(h);
            }

            return h;
        }

        public bool SpecialCharacterName(string characterName)
        {
            return characterName == PotName || characterName == DealerName || characterName == DiscardName || characterName == BurnCardsName || characterName == HouseName;
        }

        public string GetCharacterNameString(string characterName)
        {
            return SpecialCharacterName(characterName) ? Utils.GetCharacterStringFromSpecialName(characterName) : Utils.GetCharacterUserTags(characterName);
        }

        public string GetDeckKey(string channelId, DeckType deckType)
        {
            return channelId + "_" + deckType;
        }

        #region game sessions
        public string JoinGame(string characterName, string channelId, IGame gameType)
        {
            GameSession sesh = GetGameSession(channelId, gameType, true);
            if (sesh != null)
            {
                sesh.Players.Add(characterName);
                return Utils.GetCharacterUserTags(characterName) + " joined " + sesh.CurrentGame.GetGameName() + " successfully.";
            }

            return "";
        }

        public string LeaveGame(string characterName, string channelId, IGame gameType)
        {
            GameSession sesh = GetGameSession(channelId, gameType, false);
            if (sesh != null)
            {
                sesh.Players.RemoveAll(a => a == characterName);
                return Utils.GetCharacterUserTags(characterName) + " left " + sesh.CurrentGame.GetGameName() + " successfully.";
            }

            return "";
        }

        public string AddGameData(string channelId, IGame gameType, object gameData)
        {
            GameSession sesh = GetGameSession(channelId, gameType, true);
            if (sesh != null)
            {
                bool success = sesh.AddGameData(gameData);
                if(success)
                {
                    return "success";
                }
                else
                {
                    return "failed to add game data.";
                }
            }

            return "";
        }

        public string StartGame(string channelId, IGame gameType, BotMain botMain, bool keepSession, bool endSession)
        {
            GameSession sesh = GetGameSession(channelId, gameType, false);

            bool keepingSession = keepSession || (gameType.KeepSessionDefault() && !endSession);
            string output = "";
            if(sesh != null)
            {
                string runGameString = sesh.RunGame(this, botMain);

                string endingString = "\n" + sesh.CurrentGame.GetEndingDisplay();
                if(sesh.State == GameState.Finished)
                {
                    if (keepingSession)
                    {
                        sesh.State = GameState.Unstarted;

                        if(sesh.Ante > 0)
                        {
                            List<string> leavePlayers = new List<string>();
                            foreach(string characterName in sesh.Players)
                            {
                                ChipPile p = GetChipPile(characterName, channelId, false);

                                if(sesh.Ante > p.Chips)
                                {
                                    leavePlayers.Add(characterName);
                                }
                            }

                            if(leavePlayers.Count > 0)
                            {
                                string leavingCharacters = Utils.PrintList(leavePlayers) + " removed for being below the minimum ante.";
                                endingString += leavingCharacters;
                                foreach(string chara in leavePlayers)
                                {
                                    LeaveGame(chara, channelId, gameType);
                                }
                            }
                        }

                        endingString += " [i](game session kept)[/i]";
                    }
                    else
                    {
                        RemoveGameSession(channelId, gameType);
                        endingString += " [i](game session ended)[/i]";
                    }
                }
                if(sesh.State == GameState.GameInProgress)
                {
                    endingString += " [i](game session in progress)[/i]";
                }

                output = runGameString + endingString;
            }
            else
            {
                output = "Game session not found.";
            }

            return output;
        }

        public string CancelGame(string channelId, IGame gameType)
        {
            GameSession sesh = GetGameSession(channelId, gameType, false);
            string output = "";
            if (sesh != null)
            {
                RemoveGameSession(channelId, gameType);
                output = "Session for " + sesh.CurrentGame.GetGameName() + " cancelled.";
            }
            else
            {
                output = "Game session not found.";
            }

            return output;
        }

        public GameSession GetGameSession(string channelId, IGame gameType, bool createNew = true)
        {
            GameSession rtn = GameSessions.FirstOrDefault(a => a.ChannelId == channelId && a.CurrentGame.GetGameName() == gameType.GetGameName());
            if (rtn == null && createNew)
            {
                rtn = new GameSession();
                rtn.ChannelId = channelId;
                rtn.CurrentGame = gameType;
                rtn.Players = new List<string>();
                rtn.State = GameState.Unstarted;

                GameSessions.Add(rtn);
            }

            return rtn;
        }

        public string RemoveGameSession(string channelId, IGame gameType)
        {
            GameSession g = GetGameSession(channelId, gameType, false);
            if(g != null)
            {
                GameSessions.RemoveAll(a => a.CurrentGame.GetGameName() == gameType.GetGameName() && a.ChannelId == channelId);

                return "Game session removed for " + gameType.GetGameName() + ".";
            }

            return "Game session not found for " + gameType.GetGameName() + ".";
        }

        public string IssueGameCommand(string character, string channel, GameSession session, string[] terms)
        {
            string rtn = session.IssueGameCommand(this, botMain, character, channel, terms);

            return rtn;
        }

        #endregion

        #region countdown timers

        public bool CountdownFinishedOrNotStarted(string channel, string countdownId)
        {
            CountdownTimer timer = GetCountdownTimer(channel, countdownId);
            if(timer == null)
            {
                return true;
            }
            else if (timer.TimerFinished())
            {
                timer.StopTimer();
                CountdownTimers.Remove(timer);
                return true;
            }

            return false;
        }

        public CountdownTimer GetCountdownTimer(string channel, string countdownId)
        {
            return CountdownTimers.FirstOrDefault(a => a.ChannelId == channel && a.TimerId == countdownId);
        }

        public double GetSecondsRemainingOnCountdownTimer(string channel, string countdownId)
        {
            CountdownTimer timer = GetCountdownTimer(channel, countdownId);
            double secondsRemain = 0;

            if (timer != null)
                secondsRemain = timer.GetSecondsRemaining();
            return secondsRemain;
        }

        public void StartCountdownTimer(string channel, string countdownId, string characterId, int targetMiliseconds)
        {
            CountdownTimer timer = GetCountdownTimer(channel, countdownId);
            if(timer == null)
            {
                CountdownTimer t = new CountdownTimer()
                {
                    ChannelId = channel,
                    CharacterId = characterId,
                    TimerId = countdownId,
                    FinishedMs = targetMiliseconds
                };
                t.StartTimer();
                CountdownTimers.Add(t);
            }
        }

        #endregion
    }

    public enum CardMoveType
    {
        NONE,
        DiscardCard,
        PlayCard,
        ToDeckFromHand,
        ToDeckFromPlay,
        ToDeckFromDiscard,
        ToPlayFromDiscard,
        ToDiscardFromPlay,
        ToHandFromDiscard,
        ToHandFromPlay
    }
}
