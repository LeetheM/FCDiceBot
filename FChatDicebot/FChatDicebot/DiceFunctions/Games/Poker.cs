using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChatDicebot.DiceFunctions
{
    public class Poker : IGame
    {
        private const int RoundEndingWaitMsPerPlayer = 1000;
        private const int RoundEndingWaitMs = 1000 * 2;

        public string GetGameName()
        {
            return "Poker";
        }

        public int GetMaxPlayers()
        {
            return 8;
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
            return true; //this is required for setting the ante in game startup
        }

        public bool KeepSessionDefault()
        {
            return true;
        }

        public int GetMinimumMsBetweenGames()
        {
            return 0;
        }

        public string GetStartingDisplay()
        {
            return "[eicon]dbpoker1[/eicon][eicon]dbpoker2[/eicon][eicon]dbpoker3[/eicon]";
        }

        public string GetEndingDisplay()
        {
            return "";
        }

        public string GameStatus(GameSession session)
        {
            if(session.PokerData != null)
            {
                string output = "Rules: " + session.PokerData.Rules + ", " + session.PokerData.decksNumber + " decks used, small blind " + (session.Ante / 2)  + " big blind " + session.Ante + " chips.";

                if (session.PokerData.PokerPlayers != null && session.PokerData.PokerPlayers.Count > 0)
                {
                    if (output != null)
                        output += "\n";
                    output += "Current Players:\n" + session.PokerData.PrintPlayers();

                    output += "\n" + PrintCurrentPlayerTurn(session);
                }
                
                return output;
            }
            else
                return "(players and bets not found)";
        }

        public bool AddGameDataForPlayerJoin(string characterName, GameSession session, BotMain botMain, string[] terms, int ante, out string messageString)
        {
            string outputString = "";
            if(!session.PokerData.RulesSet && terms != null && terms.Count() > 0)
            {
                int deckNumber = 1;
                if (terms.Contains("x1") || terms.Contains("single"))
                    deckNumber = 1;
                if (terms.Contains("x2") || terms.Contains("double"))
                    deckNumber = 2;
                if (terms.Contains("x3") || terms.Contains("triple"))
                    deckNumber = 3;
                if (terms.Contains("x4") || terms.Contains("quadrouple"))
                    deckNumber = 4;

                PokerRuleset ruleset = PokerRuleset.FiveCardStud;
                if (terms.Contains("5carddraw") || terms.Contains("draw"))
                    ruleset = PokerRuleset.FiveCardDraw;
                if (terms.Contains("5cardstud") || terms.Contains("stud"))
                    ruleset = PokerRuleset.FiveCardStud;

                session.PokerData.RulesSet = true;
                session.PokerData.decksNumber = deckNumber;
                session.PokerData.Rules = ruleset;

                outputString += "Rules set: (game type: " + ruleset + ") (decks: " + deckNumber + ") (ante: " + (ante / 2) + " small / " + ante + " big)";
            }

            messageString = outputString;
            return true;
        }

        public string RunGame(System.Random r, String executingPlayer, List<String> playerNames, DiceBot diceBot, BotMain botMain, GameSession session)
        {
            string outputString = "[i]Setting up Poker...[/i]";

            bool resetDeck = false;

            if (session.PokerData.PokerPlayers != null && session.PokerData.PokerPlayers.Count > 0)
            {
                Deck d = diceBot.GetDeck(session.ChannelId, DeckType.Playing, null);
                int minCards = session.PokerData.Rules == PokerRuleset.FiveCardStud ? session.PokerData.PokerPlayers.Count() * 5 : 0;
                
                switch(session.PokerData.Rules)
                {
                    case PokerRuleset.FiveCardStud:
                        minCards = session.PokerData.PokerPlayers.Count() * 5;
                        break;
                    case PokerRuleset.FiveCardDraw:
                        minCards = session.PokerData.PokerPlayers.Count() * 10;
                        break;
                }
                if (d.GetCardsRemaining() < minCards)
                    resetDeck = true;
            }
            else
            {
                    resetDeck = true;
            }
            if(resetDeck)
                diceBot.ResetDeck(false, session.PokerData.decksNumber, session.ChannelId, DeckType.Playing, null);

            session.PokerData.PokerPlayers = new List<PokerPlayer>();
            //set up starting data 
            foreach(string playerName in session.Players)
            {
                session.PokerData.PokerPlayers.Add(new PokerPlayer()
                {
                    Active = true,
                    BetAmount = 0,
                    HandEvaluation = null,
                    PlayerHand = null,
                    Folded = false,
                    AllIn = false,
                    CannotAfford = false,
                    HasActedThisRound = false,
                    PlayerName = playerName
                });
            }

            if(session.State == GameState.Finished)
            {
                session.PokerData.ShiftPlayersLeft();
            }
            else
            {
                session.PokerData.ShufflePlayers(diceBot.random);
            }

            session.PokerData.CurrentBetLevel = session.Ante;
            session.PokerData.CurrentPotTotal = 0;

            bool smallBlind = true;
            //put in all antes
            foreach(PokerPlayer bet in session.PokerData.PokerPlayers)
            {
                ChipPile pile = diceBot.GetChipPile(bet.PlayerName, session.ChannelId, false);

                if(pile.Chips >= session.Ante)
                {
                    bet.CannotAfford = false;

                    if(smallBlind)
                    {
                        IncreaseBet(diceBot, botMain, session, session.ChannelId, bet, session.Ante / 2);
                        smallBlind = false;
                    }
                    else
                    {
                        IncreaseBet(diceBot, botMain, session, session.ChannelId, bet, session.Ante);
                    }
                }
                else
                {
                    bet.HasActedThisRound = true;
                    bet.CannotAfford = true;
                }
            }
            //don't claim pot with dealer (although we could...) //TODO: make a poker bets pot entity and use that instead of normal pot
            botMain.BotCommandController.SaveChipsToDisk("Poker setup");

            string drawOut = "";
            foreach(PokerPlayer bet in session.PokerData.PokerPlayers)
            {
                int initialCardsDrawn = 5;

                if(!bet.CannotAfford)
                {
                    diceBot.DrawCards(initialCardsDrawn, false, true, session.ChannelId, DeckType.Playing, bet.PlayerName, false, out drawOut);
                    Hand h2 = diceBot.GetHand(session.ChannelId, DeckType.Playing, bet.PlayerName);
                    bet.PlayerHand = h2;

                    botMain.SendPrivateMessage("Poker hand drawn: " + h2.ToString(), bet.PlayerName);
                }
            }

            string playersPrint = session.PokerData.PrintPlayers();

            session.PokerData.PokerPlayers.RemoveAll(ab => ab.CannotAfford);

            if(session.PokerData.PokerPlayers.Count == 0)
                return outputString + "\n" + playersPrint + "\n[b]all players were removed for failing to meet ante[/b].";
            else if (session.PokerData.PokerPlayers.Count < GetMinPlayers())
            {
                foreach(PokerPlayer player in session.PokerData.PokerPlayers)
                {
                    if(player.BetAmount > 0)
                    {
                        diceBot.GiveChips(DiceBot.PotPlayerAlias, player.PlayerName, session.ChannelId, player.BetAmount, false);
                        player.BetAmount = 0;
                    }
                }
                return outputString + "\n" + playersPrint + "\n[b]there are not enough players left to play who can meet ante[/b]. Ante bets have been returned.";
            }

            session.PokerData.currentPlayerIndex = 0;
            string currentTurn = PrintCurrentPlayerTurn(session);

            session.State = DiceFunctions.GameState.GameInProgress;
            
            return outputString + "\n" + playersPrint + "\n" + currentTurn + " [sub]!gc raise ###, !gc call, !gc fold, !gc allin[/sub]";
        }

        public string PlayerLeftGame(BotMain botMain, GameSession session, string characterName)
        {
            string outputstring = "Goodbye " + Utils.GetCharacterUserTags( characterName);

            if (session.PokerData.PokerPlayers.Count(c => c.PlayerName == characterName) > 0)
            {
                PokerPlayer thisPlayer = session.PokerData.PokerPlayers.FirstOrDefault(d => d.PlayerName == characterName);

                int indexOfPlayer = session.PokerData.PokerPlayers.IndexOf(thisPlayer);

                if (thisPlayer.BetAmount > 0)
                {
                    botMain.DiceBot.GiveChips(DiceBot.PotPlayerAlias, thisPlayer.PlayerName, session.ChannelId, thisPlayer.BetAmount, false);
                    session.PokerData.CurrentPotTotal -= thisPlayer.BetAmount;

                    outputstring += ", your bet amount of " + thisPlayer.BetAmount + " was returned.";
                    botMain.BotCommandController.SaveChipsToDisk("PokerPlayerLeft");
                }

                session.PokerData.PokerPlayers.Remove(thisPlayer);

                if (indexOfPlayer < session.BlackjackGameData.currentPlayerIndex)
                {
                    session.PokerData.currentPlayerIndex -= 1;
                }
                else if (indexOfPlayer == session.PokerData.currentPlayerIndex)
                {
                    session.PokerData.currentPlayerIndex -= 1;
                    PassTurnToNextPlayer(botMain.DiceBot, botMain, session, session.ChannelId);

                    if(session.State != GameState.Finished) //round not finished by passing turn
                    {
                        outputstring += ", " + PrintCurrentPlayerTurn(session);
                    }
                }
            }

            return outputstring;
        }

        public int GetPokerNumberScoreForCard(DeckCard d)
        {
            if (d.number == 1)
                return 14;
            else
                return d.number;
        }

        public PokerHandResult EvaluatePokerHand(Hand h, bool folded)
        {
            PokerHandResult result = new PokerHandResult();

            if (h != null && h.CardsCount() >= 1)
            {
                List<DeckCard> allHandCards = new List<DeckCard>();
                for(int i = 0; i < h.CardsCount(); i++)
                {
                    allHandCards.Add(h.GetCardAtIndex(i));
                }

                var groupedCardsByNumber = allHandCards.GroupBy(card => card.number);

                PokerHandType evaluatedType = PokerHandType.HighCard;
                long scoreBonus = 0;
                long handTypeMultiplier = 100000000000;
                long firstTiebreak = 1000000000;
                long secondTiebreak = 10000000;
                long thirdTiebreak = 100000;
                long fourthTiebreak = 1000;
                long fifthTiebreak = 10;
                //find if there are 2 of a kind
                var twoSameNumber = groupedCardsByNumber.Where(group => group.Count() == 2);
                if (twoSameNumber != null && twoSameNumber.Count() == 1)
                {
                    DeckCard pairCard = twoSameNumber.ElementAt(0).First();
                    scoreBonus = GetPokerNumberScoreForCard(pairCard) * firstTiebreak; //1 mil
                    List<DeckCard> restOfHand = allHandCards.Where(a => a.number != pairCard.number).OrderByDescending(a => GetPokerNumberScoreForCard(a)).ToList();// a.number).ToList();
                    scoreBonus += GetPokerNumberScoreForCard(restOfHand[0]) * thirdTiebreak;
                    scoreBonus += GetPokerNumberScoreForCard(restOfHand[1]) * fourthTiebreak;
                    scoreBonus += GetPokerNumberScoreForCard(restOfHand[2]) * fifthTiebreak;
                    evaluatedType = PokerHandType.OnePair;
                }
                if (twoSameNumber != null && twoSameNumber.Count() == 2)
                {
                    DeckCard pairCard1 = twoSameNumber.ElementAt(0).First();
                    DeckCard pairCard2 = twoSameNumber.ElementAt(1).First();
                    long scorebonus1 = GetPokerNumberScoreForCard(pairCard1);
                    long scorebonus2 = GetPokerNumberScoreForCard(pairCard2);
                    long bigBonus = Math.Max(scorebonus1, scorebonus2);
                    long smallBonus = Math.Min(scorebonus1, scorebonus2);
                    scoreBonus = bigBonus * firstTiebreak + smallBonus * secondTiebreak;
                    DeckCard lastCard = allHandCards.FirstOrDefault(a => a.number != pairCard1.number && a.number != pairCard2.number);
                    scoreBonus += GetPokerNumberScoreForCard(lastCard) * thirdTiebreak;

                    evaluatedType = PokerHandType.TwoPair;
                }

                var threeSameNumber = groupedCardsByNumber.FirstOrDefault(group => group.Count() == 3);
                if (threeSameNumber != null && threeSameNumber.Count() == 1)
                {
                    DeckCard pairCard1 = threeSameNumber.First();
                    scoreBonus = GetPokerNumberScoreForCard(pairCard1) * firstTiebreak;
                    List<DeckCard> restOfHand = allHandCards.Where(a => a.number != pairCard1.number).OrderByDescending(a => GetPokerNumberScoreForCard(a)).ToList();// a.number).ToList();
                    scoreBonus += GetPokerNumberScoreForCard(restOfHand[0]) * secondTiebreak;
                    scoreBonus += GetPokerNumberScoreForCard(restOfHand[1]) * thirdTiebreak;
                    
                    evaluatedType = PokerHandType.ThreeOfAKind;
                }
                if (threeSameNumber != null && threeSameNumber.Count() > 0 && twoSameNumber != null && twoSameNumber.Count() == 1)
                {

                    DeckCard pairCard1 = threeSameNumber.First();
                    DeckCard pairCard2 = twoSameNumber.ElementAt(0).First();
                    long scorebonus1 = GetPokerNumberScoreForCard(pairCard1);
                    long scorebonus2 = GetPokerNumberScoreForCard(pairCard2);
                    scoreBonus = scorebonus1 * firstTiebreak + scorebonus2 * secondTiebreak;

                    evaluatedType = PokerHandType.FullHouse;
                }

                var fourSameNumber = groupedCardsByNumber.FirstOrDefault(group => group.Count() == 4);
                if (fourSameNumber != null && fourSameNumber.Count() > 0)
                {
                    evaluatedType = PokerHandType.FourOfAkind;
                    DeckCard pairCard1 = fourSameNumber.First();
                    DeckCard lastCard = allHandCards.FirstOrDefault(a => a.number != pairCard1.number);
                    scoreBonus += GetPokerNumberScoreForCard(pairCard1) * firstTiebreak;
                    scoreBonus += GetPokerNumberScoreForCard(lastCard) * secondTiebreak;
                }

                if (allHandCards.Count(a => a.suit == allHandCards[0].suit) == allHandCards.Count())
                {
                    evaluatedType = PokerHandType.Flush;
                    List<DeckCard> restOfHand = allHandCards.OrderByDescending(a => GetPokerNumberScoreForCard(a)).ToList();// a.number).ToList();
                    scoreBonus += GetPokerNumberScoreForCard(restOfHand[0]) * firstTiebreak;
                    scoreBonus += GetPokerNumberScoreForCard(restOfHand[1]) * secondTiebreak;
                    scoreBonus += GetPokerNumberScoreForCard(restOfHand[2]) * thirdTiebreak;
                    scoreBonus += GetPokerNumberScoreForCard(restOfHand[3]) * fourthTiebreak;
                    scoreBonus += GetPokerNumberScoreForCard(restOfHand[4]) * fifthTiebreak;
                }

                bool hasStraight = false;
                var uniqueNumber = groupedCardsByNumber.Where(group => group.Count() == 1);
                bool hasAce = allHandCards.Count(a => a.number == 1) > 0;
                int minNumber = allHandCards.Min(a => a.number);
                int maxNumber = allHandCards.Max(a => a.number);
                int countOver9 = allHandCards.Count(a => a.number > 9);
                if (uniqueNumber.Count() == 5 && ((minNumber + 5 > maxNumber) || (hasAce && countOver9 == 4)))
                {
                    hasStraight = true;
                }

                if(hasStraight)
                {
                    evaluatedType = PokerHandType.Straight;

                    var highestCard = allHandCards.Max(a => GetPokerNumberScoreForCard(a));
                    scoreBonus = firstTiebreak * highestCard;

                    if(allHandCards.Count(a => a.suit == allHandCards[0].suit) == allHandCards.Count())
                    {
                        evaluatedType = PokerHandType.StraightFlush;

                        if (hasAce && allHandCards.Count(a => a.number > 9) == 4)
                            evaluatedType = PokerHandType.RoyalFlush;
                    }
                }

                if(evaluatedType == PokerHandType.HighCard)
                {
                    var orderedHand = allHandCards.OrderByDescending(a => GetPokerNumberScoreForCard(a)).ToList();
                    long bonus = GetPokerNumberScoreForCard(orderedHand[0]) * firstTiebreak +
                        GetPokerNumberScoreForCard(orderedHand[1]) * secondTiebreak +
                        GetPokerNumberScoreForCard(orderedHand[2]) * thirdTiebreak +
                        GetPokerNumberScoreForCard(orderedHand[3]) * fourthTiebreak +
                        GetPokerNumberScoreForCard(orderedHand[4]) * fifthTiebreak;
                    scoreBonus = bonus;
                }

                result.HandType = evaluatedType;

                result.score = ((int)evaluatedType + 1) * handTypeMultiplier; 

                result.score = result.score + scoreBonus;
            }

            if (folded)
                result.score = 0;

            return result;
        }

        public void PassTurnToNextPlayer(DiceBot diceBot, BotMain botMain, GameSession session, string channel)
        {
            PokerData data = session.PokerData;

            if(data != null && data.PokerPlayers != null)
            {
                PokerPlayer thisPlayer = data.GetCurrentPlayer();
                thisPlayer.HasActedThisRound = true;
                data.currentPlayerIndex += 1;
                if (data.currentPlayerIndex >= data.PokerPlayers.Count)
                {
                    data.currentPlayerIndex = 0;
                }

                data.ForceStandVotes = new List<String>();

                if (PlayersAllFinishedThisRound(session))
                {
                    FinishRound(diceBot, botMain, session, channel);
                }
                else if (thisPlayer.Folded || thisPlayer.AllIn || thisPlayer.CannotAfford)
                {
                    PassTurnToNextPlayer(diceBot, botMain, session, channel);
                }
            }
        }

        public void FinishRound(DiceBot diceBot, BotMain botMain, GameSession session, string channel)
        {
            string outputString = "";
            string playerHandsString = "";
            if (session.PokerData.PokerPlayers != null)
            {
                //score each hand to check whose hand is the best
                foreach (PokerPlayer player in session.PokerData.PokerPlayers)
                {
                    player.HandEvaluation = EvaluatePokerHand(player.PlayerHand, player.Folded);

                    if (!string.IsNullOrEmpty(playerHandsString))
                        playerHandsString += "\n";
                    playerHandsString += Utils.GetCharacterUserTags(player.PlayerName) + " 's hand : " + player.PlayerHand.ToString(false, false) + " (" + player.HandEvaluation.ToString() + ") " + 
                        ((player.Folded || player.CannotAfford)? "[color=gray](folded)[/color]":"[color=green](active)[/color] ") + (player.AllIn? "[color=red](all in)[/color]":"");
                }

                List<PokerPlayer> playerHandsRanked = new List<PokerPlayer>();
                playerHandsRanked = session.PokerData.PokerPlayers.OrderByDescending(a => a.HandEvaluation.score).ToList();

                int remainingPot = session.PokerData.CurrentPotTotal;
                int currentHandIndex = 0;

                List<PokerBetNonsense> payoutAmounts = new List<PokerBetNonsense>();
                //split the pot according to who won
                while(remainingPot > 0 && currentHandIndex < playerHandsRanked.Where(a => a.HandEvaluation.score > 0).Count())
                {
                    //determine who gets paid
                    List<PokerPlayer> currentPaid = playerHandsRanked.Where(a => a.HandEvaluation.score == playerHandsRanked[currentHandIndex].HandEvaluation.score).ToList();

                    List<PokerPlayer> eligibleToPay = playerHandsRanked.Where(a => a.HandEvaluation.score < currentPaid[0].HandEvaluation.score).ToList(); //eligible players to pay 
                    //determine maximum payout
                    int potPayout = remainingPot / currentPaid.Count();
                    int leftover = 0;
                    for(int i = 0; i < currentPaid.Count(); i++)
                    {
                        PokerPlayer player = currentPaid[i];
                        int maxPayout = currentPaid[0].BetAmount + eligibleToPay.Sum(a => Math.Max(currentPaid[0].BetAmount, a.BetAmount)); //paying up to their max bet # each up to the max payout of this player

                        if(maxPayout < potPayout)
                        {
                            leftover += potPayout - maxPayout;
                        }

                        int amountPaid = Math.Min(maxPayout, potPayout);
                        payoutAmounts.Add(new PokerBetNonsense() { AmountPaid = amountPaid, MaxPayout = maxPayout, CharacterName = player.PlayerName });
                    }

                    if(leftover > 0)
                    {
                        List<PokerBetNonsense> betnonsenseTemp = new List<PokerBetNonsense>(payoutAmounts);
                        while(betnonsenseTemp.Count() > 0)
                        {
                            betnonsenseTemp.RemoveAll(a => a.MaxPayout <= a.AmountPaid);
                            int remainingSplit = leftover / betnonsenseTemp.Count();

                            foreach(PokerBetNonsense player in betnonsenseTemp)
                            {
                                int amountAdded = Math.Min(remainingSplit, player.MaxPayout - player.AmountPaid);
                                player.AmountPaid += amountAdded;
                                leftover -= amountAdded;
                            }
                        }
                    }
                    foreach(PokerBetNonsense thispaid in payoutAmounts)
                    {
                        if(currentPaid.Select(a => a.PlayerName).Contains(thispaid.CharacterName))
                        {
                            remainingPot -= thispaid.AmountPaid;
                        }
                    }

                    currentHandIndex += currentPaid.Count();
                }

                foreach (PokerBetNonsense payoutAward in payoutAmounts)
                {
                    if (!string.IsNullOrEmpty(outputString))
                        outputString += "\n";

                    outputString += Utils.GetCharacterUserTags(payoutAward.CharacterName) + " is awarded " + payoutAward.AmountPaid + " chips.";
                    diceBot.GiveChips(DiceBot.PotPlayerAlias, payoutAward.CharacterName, channel, payoutAward.AmountPaid, false);
                }

                diceBot.ClaimPot(DiceBot.DealerPlayerAlias, channel, 1); //remove extras from pot

                diceBot.EndHand(channel, false, DeckType.Playing);
                //save chips modifications to disk
                botMain.BotCommandController.SaveChipsToDisk("Pokerroundfinish");
            }

            string fullPotString = "The pot contains [color=yellow]" + session.PokerData.CurrentPotTotal + " chips[/color].";
            botMain.SendFutureMessage("[color=yellow]Round Finished![/color]\n" + fullPotString + "\n" + playerHandsString + "\n" + outputString, channel, null, true, RoundEndingWaitMsPerPlayer * session.PokerData.PokerPlayers.Count() + RoundEndingWaitMs);
            session.State = GameState.Finished;
        }

        public string DrawCards(DiceBot diceBot, string channel, string character, List<int> cardIndexes, out string truedraw)
        {
            int discardNumber = 0;
            diceBot.DiscardCards(cardIndexes, false, channel, DeckType.Playing, character, out discardNumber);

            string drawString = diceBot.DrawCards(cardIndexes.Count(), false, true, channel, DeckType.Playing, character, false, out truedraw);

            return Utils.GetCharacterStringFromSpecialName(character) + " discarded " + discardNumber + " cards to draw. " + drawString;
        }

        public string IncreaseBet(DiceBot diceBot, BotMain botMain, GameSession session, string channel, PokerPlayer player, int additionalBetAmount)
        {
            player.BetAmount = player.BetAmount + additionalBetAmount;

            ChipPile pile = diceBot.GetChipPile(player.PlayerName, channel, false);
            bool allin = false;
            if(pile.Chips < additionalBetAmount)
            {
                additionalBetAmount = pile.Chips;
                allin = true;
            }

            string giveChipsOutput = diceBot.GiveChips(player.PlayerName, DiceBot.PotPlayerAlias, channel, additionalBetAmount, allin);
            session.PokerData.CurrentPotTotal += additionalBetAmount;

            if (player.BetAmount > session.PokerData.CurrentBetLevel)
                session.PokerData.CurrentBetLevel = player.BetAmount;

            return giveChipsOutput;
        }

        public string PrintCurrentPlayerTurn(GameSession session)
        {
            if (PlayersAllFinishedThisRound(session))
                return "(The round has finished)";
            else
                return session.PokerData.PrintCurrentPlayerTurn();
        }

        public bool PlayersAllFinishedThisRound(GameSession session)
        {
            return session.PokerData.PokerPlayers.Count(a => !a.HasActedThisRound || (a.BetAmount < session.PokerData.CurrentBetLevel && !a.AllIn && !a.CannotAfford && a.Active)) == 0;
        }

        public string IssueGameCommand(DiceBot diceBot, BotMain botMain, string character, string channel, GameSession session, string[] terms, string[] rawTerms)
        {
            string returnString = "";

            PokerPlayer currentPlayer = session.PokerData.GetCurrentPlayer();
            bool currentPlayerIssuedCommand = currentPlayer != null && currentPlayer.PlayerName == character;

            if (terms.Contains("help"))
            {
                returnString = "The current player can use !gc call, fold, raise, allin. Other commands: currentturn, turn, setminbet, setante . Startup options: ### (sets your bet amount), 5carddraw/5cardstud/texasholdem (sets ruleset), minbet### (sets min bet), x1/x2/x3/x4 (sets deck count)";
            }
            else if (terms.Contains("forcestand") || terms.Contains("forcecheck"))
            {
                var bp = session.PokerData.GetPlayer(character);

                if (session.State != GameState.GameInProgress)
                    returnString = "This command can only be used while the game is in progress.";
                else if (session.PokerData.ForceStandVotes.Contains(character))
                    returnString = "Each player in the game only has one vote to force stand.";
                else
                {
                    bool characterIsAdmin = Utils.IsCharacterTrusted(botMain.AccountSettings.TrustedCharacters, character, channel) 
                        || Utils.IsCharacterAdmin(botMain.AccountSettings.AdminCharacters, character);
                    if (characterIsAdmin)
                    {
                        session.PokerData.ForceStandVotes.Add(character);
                        session.PokerData.ForceStandVotes.Add(character);
                        session.PokerData.ForceStandVotes.Add(character);
                        session.PokerData.ForceStandVotes.Add(character);
                    }
                    session.PokerData.ForceStandVotes.Add(character);

                    int currentVotes = session.PokerData.ForceStandVotes.Count();
                    int requiredVotes = Math.Max(2, session.PokerData.PokerPlayers.Count() / 2);

                    if (currentVotes >= requiredVotes)
                    {
                        string betChangeAmount = "";

                        if (session.PokerData.CurrentBetLevel > currentPlayer.BetAmount)
                        {
                            betChangeAmount = IncreaseBet(diceBot, botMain, session, channel, currentPlayer, session.PokerData.CurrentBetLevel - currentPlayer.BetAmount);
                        }

                        string forcedCharacter = session.PokerData.GetCurrentPlayer().PlayerName;
                        PassTurnToNextPlayer(diceBot, botMain, session, channel);

                        returnString = Utils.GetCharacterUserTags(forcedCharacter) + " [color=cyan]checks[/color]. They will meet the current bet of " + session.PokerData.CurrentBetLevel + ".\n"
                            + betChangeAmount + " " + PrintCurrentPlayerTurn(session);
                    }
                    else
                        returnString = Utils.GetCharacterUserTags(character) + " has voted to force stand: " + currentVotes + " / " + (requiredVotes) + " votes.";
                }
            }
            //TODO: handle redraw for 5 card draw games
            else if (terms.Contains("fold") || terms.Contains("surrender") || terms.Contains("call") || terms.Contains("check") || terms.Contains("stand") 
                || terms.Contains("raise") || terms.Contains("bet") || terms.Contains("allin") || terms.Contains("all-in") || terms.Contains("all"))
            {
                if (session.State != GameState.GameInProgress)
                    returnString = "Failed: This command can only be used while the game is in progress.";
                else if (currentPlayer == null)
                    returnString = "Error: Current player not found.";
                else if (!currentPlayerIssuedCommand)
                    returnString = "Failed: Only the current player can access this command.";
                else
                {
                    //fold / surrender?
                    if (terms.Contains("fold") || terms.Contains("surrender"))
                    {
                        currentPlayer.Folded = true;

                        PassTurnToNextPlayer(diceBot, botMain, session, channel);
                        returnString = Utils.GetCharacterUserTags(character) + " [color=gray]folds[/color]. " + PrintCurrentPlayerTurn(session);
                    }

                    //stand
                    if (terms.Contains("call") || terms.Contains("check") || terms.Contains("stand"))
                    {
                        string betChangeAmount = "";

                        if(session.PokerData.CurrentBetLevel > currentPlayer.BetAmount)
                        {
                            betChangeAmount = IncreaseBet(diceBot, botMain, session, channel, currentPlayer, session.PokerData.CurrentBetLevel - currentPlayer.BetAmount);
                        }

                        PassTurnToNextPlayer(diceBot, botMain, session, channel);
                        returnString = Utils.GetCharacterUserTags(character) + " [color=cyan]checks[/color]. They will meet the current bet of " + session.PokerData.CurrentBetLevel + ".\n" 
                            + betChangeAmount + " " + PrintCurrentPlayerTurn(session);
                    }

                    //raise
                    if (terms.Contains("raise") || terms.Contains("bet") || terms.Contains("allin") || terms.Contains("all-in") || terms.Contains("all"))
                    {
                        //first call the current raise
                        if (session.PokerData.CurrentBetLevel > currentPlayer.BetAmount)
                        {
                            IncreaseBet(diceBot, botMain, session, channel, currentPlayer, session.PokerData.CurrentBetLevel - currentPlayer.BetAmount);
                        }
                        //now determine how much can be raised
                        int amountRaised = 0;
                        var cp = diceBot.GetChipPile(currentPlayer.PlayerName, channel, false);

                        if(terms.Contains("allin") || terms.Contains("all-in") || terms.Contains("all"))
                        {
                            amountRaised = cp.Chips;
                        }
                        else
                        {
                            amountRaised = Utils.GetNumberFromInputs(terms);
                        }

                        if (amountRaised > 0 && currentPlayer.HasActedThisRound)
                        {
                            returnString = "Failed: you have already acted this round. You may not raise again.";
                        }
                        else if(amountRaised > cp.Chips)
                        {
                            //error
                            returnString = "Failed: you do not have enough chips in your pile to raise. Try allin. (requested " + amountRaised + ")(held " + cp.Chips + ")";
                        }
                        else
                        {
                            string outputFromIncreasedBet = IncreaseBet(diceBot, botMain, session, channel, currentPlayer, amountRaised);

                            PassTurnToNextPlayer(diceBot, botMain, session, channel);
                            returnString = Utils.GetCharacterUserTags(character) + " has raised by " + amountRaised +
                                ". The current total bet to meet is " + session.PokerData.CurrentBetLevel + " chips.\n" + 
                                outputFromIncreasedBet + " " + (currentPlayer.AllIn? "[color=red]all in[/color] ":"") + PrintCurrentPlayerTurn(session);
                        }
                    }

                }

            }
            else if (terms.Contains("currentturn") || terms.Contains("showturn"))
            {
                if (session.State != GameState.GameInProgress)
                    returnString = "Failed: This command can only be used while the game is in progress.";
                else
                    returnString = "Current turn for Poker: " + PrintCurrentPlayerTurn(session);
            }
            else if (terms.Contains("setminbet") || terms.Contains("setante"))
            {
                int amount = Utils.GetNumberFromInputs(terms);
                if(amount >= 0)
                {
                    session.Ante = amount;
                    returnString = "Set the minimum bet for this session to " + amount;
                }
            }
            else { returnString += "No such command exists"; }

            return returnString;
        }
    }

    public class PokerData
    {
        public bool RulesSet;
        public int decksNumber = 1; //default to 1 deck
        public PokerRuleset Rules;
        //public int minBet = 0;
        public int smallBlind = 0;

        public int CurrentPotTotal = 0;
        public int CurrentBetLevel = 0;

        public List<BlackjackBet> PotBetAmounts = new List<BlackjackBet>();
        public List<PokerPlayer> PokerPlayers = new List<PokerPlayer>();

        public int currentPlayerIndex = -1;
        public int smallBlindIndex = 0;

        public List<string> ForceStandVotes = new List<string>();
        //public int forceStandVotes = -1;

        public PokerPlayer GetCurrentPlayer()
        {
            if(PokerPlayers == null || currentPlayerIndex < 0 || currentPlayerIndex > PokerPlayers.Count - 1 )
                return null;

            return PokerPlayers.ElementAt(currentPlayerIndex);
        }

        public PokerPlayer GetPlayer(string characterName)
        {
            if(PokerPlayers == null || PokerPlayers.Count == 0)
                return null;
            return PokerPlayers.FirstOrDefault(a => a.PlayerName == characterName);
        }

        public string PrintPlayers()
        {
            string rtn = "";

            if(PokerPlayers != null && PokerPlayers.Count >= 1)
            {
                bool smallBlind = true;
                foreach(PokerPlayer p in PokerPlayers)
                {
                    if (!string.IsNullOrEmpty(rtn))
                        rtn += "\n";
                    if(smallBlind)
                    {
                        rtn += p.ToString() + " (small blind)";
                        smallBlind = false;
                    }
                    else
                    {
                        rtn += p.ToString();
                    }
                } 
            }
            return rtn;
        }

        public void ShufflePlayers(System.Random r)
        {
            if(PokerPlayers != null && PokerPlayers.Count > 1)
            {
                List<PokerPlayer> tempList = new List<PokerPlayer>();
                tempList.AddRange(PokerPlayers);
                PokerPlayers = new List<PokerPlayer>();

                while(tempList.Count > 0)
                {
                    int randomIndex = r.Next(tempList.Count);
                    PokerPlayers.Add(tempList.ElementAt(randomIndex));
                    tempList.RemoveAt(randomIndex);
                }
            }
        }

        public void ShiftPlayersLeft()
        {
            if (PokerPlayers != null && PokerPlayers.Count > 1)
            {
                List<PokerPlayer> tempList = new List<PokerPlayer>();
                tempList.AddRange(PokerPlayers);
                tempList.RemoveAt(0);
                tempList.Add(PokerPlayers[0]);
                PokerPlayers = new List<PokerPlayer>();

                foreach(PokerPlayer p in tempList)
                {
                    PokerPlayers.Add(p);
                }
            }
        }

        public string PrintCurrentPlayerTurn()
        {
            string outputString = "";
            if (PokerPlayers == null || PokerPlayers.Count == 0)
                return "(no players found)";

            if (currentPlayerIndex >= PokerPlayers.Count)
                outputString += "it is now the dealer's turn.";
            else
                outputString += "it is now " + Utils.GetCharacterUserTags(GetCurrentPlayer().PlayerName) + "'s turn";

            return outputString;
        }
    }

    public class PokerHandResult
    {
        public long score;

        public bool folded;

        public PokerHandType HandType;

        public string Result;

        public override string ToString()
        {
            return HandType.ToString() + " :: score = " + score;
        }
    }

    public enum PokerHandType
    {
        HighCard,
        OnePair,
        TwoPair, //3
        ThreeOfAKind,
        Straight,
        Flush, //6
        FullHouse,
        FourOfAkind,
        StraightFlush, //9
        RoyalFlush
    }

    public class PokerPlayer
    {
        public int BetAmount;

        public Hand PlayerHand;

        public string PlayerName;

        public bool Folded;
        public bool AllIn;

        public bool HasActedThisRound;

        public bool Active;
        public PokerHandResult HandEvaluation;
        public bool CannotAfford;

        public static string GetHandString(Hand hand)
        {
            if (hand != null)
            {
                string handString = hand.ToString(true);

                return handString;
            }
            else
                return "(cards not drawn)";
        }

        public override string ToString()
        {
            string betAmount = "has bet: [b]" + BetAmount + "[/b] chips";
            if (CannotAfford)
            {
                betAmount = "(could not afford the ante and has not joined)";
            }

            return Utils.GetCharacterUserTags(PlayerName) + " " + betAmount + ", dealt" + GetHandString(PlayerHand) + "." + (Folded? "(folded)":"") + (Active ? "" : " (inactive)");
        }
    }

    public class PokerBetNonsense
    {
        public int MaxPayout;
        public int AmountPaid;
        public string CharacterName;
        public long HandScore;
        public bool Folded;

    }

    public enum PokerRuleset
    {
        FiveCardStud,
        FiveCardDraw,
        TexasHoldem,
        NCardStud,
        NCardDraw
    }
}
