using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FChatDicebot.DiceFunctions
{
    public class DiceBot
    {
        public System.Random random;
        public const int MaximumDice = 100;
        public const int MaximumSides = 100000;
        public const int MaximumCommandLength = 1000;
        public const int MaximumHandSize = 20;

        public static List<char> ValidOperators = new List<char>(){ '+', '-', '*', '/' };

        public const string DealerName = "the_dealer";
        public const string BurnCardsName = "burn_cards";

        public List<Deck> ChannelDecks;
        public List<Hand> Hands;

        public DiceBot()
        {
            random = new System.Random();

            ChannelDecks = new List<Deck>();
            Hands = new List<Hand>();
        }

        public string Roll(int diceNumber, int sidesNumber)
        {
            DiceRoll roll = new DiceRoll()
            {
                DiceRolled = diceNumber,
                DiceSides = sidesNumber
            };
            roll.Roll(random);

            return roll.ResultString();
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
                    
                    totalResultString += "= [b]" + allNumbers[0] + "[/b]";
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

        public List<string> SeparateCommands(string allCommands, out List<char> operators, out string error)
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
            int keepHighest = 0;
            int keepLowest = 0;
            int removeHighest = 0;
            int removeLowest = 0;

            string[] split = command.Split('d');
            string splitDiceNumber = split[0];
            string splitDiceSidesAndCommands = split[1];
            bool success1 = int.TryParse(splitDiceNumber, out numberDice);


            //parse out '!'
            if (splitDiceSidesAndCommands.Contains('!'))
            {
                explode = true;
                splitDiceSidesAndCommands = splitDiceSidesAndCommands.Replace("!","");
            }
            //parse out 'kh#'
            string currentParseCommand = "kh";
            if (splitDiceSidesAndCommands.Contains(currentParseCommand))
            {
                string error = "";
                string remainingString = "";
                keepHighest = keepNumberParse(splitDiceSidesAndCommands, currentParseCommand, out error, out remainingString);

                if(error != "")
                    return new DiceRoll() { Error = true, ErrorString = error };
                
                if (keepHighest > 0)
                {
                    splitDiceSidesAndCommands = remainingString;
                }
            }
            //parse out 'Kl#'
            currentParseCommand = "kl";
            if (splitDiceSidesAndCommands.Contains(currentParseCommand))
            {
                string error = "";
                string remainingString = "";
                keepLowest = keepNumberParse(splitDiceSidesAndCommands, currentParseCommand, out error, out remainingString);

                if (error != "")
                    return new DiceRoll() { Error = true, ErrorString = error };
                
                if (keepLowest > 0)
                {
                    splitDiceSidesAndCommands = remainingString;
                }
            }
            //parse out 'rh#'
            currentParseCommand = "rh";
            if (splitDiceSidesAndCommands.Contains(currentParseCommand))
            {
                string error = "";
                string remainingString = "";
                removeHighest = keepNumberParse(splitDiceSidesAndCommands, currentParseCommand, out error, out remainingString);

                if (error != "")
                    return new DiceRoll() { Error = true, ErrorString = error };
                
                if (removeHighest > 0)
                {
                    splitDiceSidesAndCommands = remainingString;
                }
            }
            //parse out 'rl#'
            currentParseCommand = "rl";
            if (splitDiceSidesAndCommands.Contains(currentParseCommand))
            {
                string error = "";
                string remainingString = "";
                removeLowest = keepNumberParse(splitDiceSidesAndCommands, currentParseCommand, out error, out remainingString);

                if (error != "")
                    return new DiceRoll() { Error = true, ErrorString = error };
                
                if (removeLowest > 0)
                {
                    splitDiceSidesAndCommands = remainingString;
                }
            }

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
                TextFormat = true,
                KeepHighest = keepHighest,
                KeepLowest = keepLowest,
                RemoveHighest = removeHighest,
                RemoveLowest = removeLowest
            };
        }

        public int keepNumberParse(string parseString, string keepCharacters, out string errorString, out string remainingString)
        {
            errorString = "";
            remainingString = "";

            int indexOfKh = parseString.IndexOf(keepCharacters);

            if (indexOfKh == -1)
                return 0;

            if (parseString.Length < indexOfKh + 2 || !char.IsDigit(parseString[indexOfKh + 2]))
            {
                errorString = keepCharacters + " must be followed by a number of dice";
                return 0;
            }

            string khNumber = "";

            string tempString = parseString.Substring(indexOfKh + 2);

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
            if (returnInt > MaximumDice)
            {
                errorString = "cannot choose more than " + MaximumDice + " dice";
                return 0;
            }
            if (parsed && returnInt <= 0)
            {
                errorString = keepCharacters + " cannot be 0 or less dice";
                return 0;
            }

            return returnInt;
        }
        
        public string DrawCards(int numberDraws, bool includeJoker, bool fromDeck, string channelId, DeckType deckType, string character)
        {
            if (numberDraws <= 0)
                numberDraws = 1;

            if (numberDraws > 10)
                numberDraws = 10;

            string totalDrawString = "";

            Hand thisCharacterHand = GetHand(channelId, deckType, character);

            bool showHand = false;
            if (!thisCharacterHand.Empty())
                showHand = true;
            
            for(int i = 0; i < numberDraws; i++)
            {
                if (!string.IsNullOrEmpty(totalDrawString))
                    totalDrawString += ", ";

                if(thisCharacterHand.CardsCount() >= MaximumHandSize)
                {
                    totalDrawString += "(maximum hand size reached!)";
                    break;
                }

                DeckCard d = DrawCard(includeJoker, fromDeck, channelId, deckType);

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
            if(showHand)
            {
                totalDrawString += "\n[i]Current Hand: [/i]" + thisCharacterHand.ToString();
            }

            return totalDrawString;
        }

        public string DiscardCards(List<int> discardsList, bool all, string channelId, DeckType deckType, string character, out int actualDiscards)
        {
            actualDiscards = 0;

            Hand thisCharacterHand = GetHand(channelId, deckType, character);

            if (thisCharacterHand.Empty())
                return "(hand was empty)";

            if (all)
            {
                discardsList = new List<int>();
                for(int i = 0; i < thisCharacterHand.CardsCount(); i++)
                {
                    discardsList.Add(i);
                }
            }

            if (discardsList == null || discardsList.Count <= 0)
                discardsList = new List<int>() { 0 };

            if (discardsList.Count > 10)
                discardsList = discardsList.Take(10).ToList();

            string totalDrawString = "";

            bool showHand = false;
            if (!thisCharacterHand.Empty())
                showHand = true;

            int currentHandSize = thisCharacterHand.CardsCount();

            List<DeckCard> newHandCards = new List<DeckCard>();

            for (int i = 0; i < currentHandSize; i++ )
            {
                if(discardsList.Contains(i))
                {
                    if (!string.IsNullOrEmpty(totalDrawString))
                        totalDrawString += ", ";

                    totalDrawString += thisCharacterHand.GetCardAtIndex(i).ToString();
                }
                else
                {
                    newHandCards.Add(thisCharacterHand.GetCardAtIndex(i));
                }
            }

            thisCharacterHand.ResetHand();

            foreach(DeckCard d in newHandCards)
            {
                thisCharacterHand.AddCard(d);
            }

            actualDiscards = currentHandSize - thisCharacterHand.CardsCount();

            if (showHand)
            {
                totalDrawString += "\n[i]Current Hand: [/i]" + thisCharacterHand.ToString();
            }

            return totalDrawString;
        }


        public DeckCard DrawCard(bool includeJoker, bool fromDeck, string channelId, DeckType deckType)
        {
            DeckCard rtnCard = new DeckCard();

            if (fromDeck)
            {
                Deck thisDeck = GetDeck(channelId, deckType);
                DeckCard drawn = thisDeck.DrawCard();

                rtnCard = drawn;
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

        public void ShuffleDeck(Random r, string channelId, DeckType deckType, bool fullShuffle)
        {
            Deck relevantDeck = GetDeck(channelId, deckType);

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

        public void ResetDeck(bool jokers, string channelId, DeckType deckType)
        {
            Deck relevantDeck = GetDeck(channelId, deckType);

            EndHand(channelId, deckType);

            relevantDeck.FillDeck(jokers);
            relevantDeck.ShuffleFullDeck(random);
        }

        public Deck GetDeck(string channelId, DeckType deckType)
        {
            string deckKey = GetDeckKey(channelId, deckType);
            Deck thisDeck = ChannelDecks.FirstOrDefault(a => a.Id == deckKey);
            if (thisDeck == null)
            {
                thisDeck = new Deck(deckType);
                thisDeck.Id = deckKey;
                thisDeck.FillDeck(false);
                thisDeck.ShuffleFullDeck(random);
                ChannelDecks.Add(thisDeck);
            }

            return thisDeck;
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
                Hands.Add(h);
            }

            return h;
        }

        public List<Hand> GetChannelHands(string channelId, DeckType deckType)
        {
            string deckKey = GetDeckKey(channelId, deckType);
            return Hands.Where(a => a.Id == deckKey).ToList();
        }

        public string GetDeckKey(string channelId, DeckType deckType)
        {
            return channelId + "_" + deckType;
        }
    }
}
