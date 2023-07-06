using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands
{
    public class ShuffleDiscardIntoDeck : ChatBotCommand
    {
        public ShuffleDiscardIntoDeck()
        {
            Name = "shufflediscardintodeck";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelDecks;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            string rtn = "";
            terms.Concat(new string[] { "all" });
            MoveCards.Run(bot, commandController, rawTerms, terms, characterName, channel, command, CardPileId.Discard, CardPileId.Deck);//, CardMoveType.ToDiscardFromPlay);
            rtn += "Transferred Discard pile into deck.";

            bool fullShuffle = false;
            if (terms != null && terms.Length >= 1 && terms.Contains("eh"))
                fullShuffle = true;

            DeckType deckType = commandController.GetDeckTypeFromCommandTerms(terms);

            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType, characterName);

            string customDeckName = Utils.GetCustomDeckName(characterName);
            bot.DiceBot.ShuffleDeck(bot.DiceBot.random, channel, deckType, fullShuffle, customDeckName);
            bot.SendMessageInChannel("[i]" + deckTypeString + rtn + " Channel deck shuffled. " + (fullShuffle ? "Hands emptied." : "") + "[/i]", channel);


            //bool all = false;
            //bool redraw = false;
            //bool secretDraw = false;
            //string characterDrawName = commandController.GetCharacterDrawNameFromCommandTerms(characterName, terms);
            //if (characterDrawName == DiceBot.DiscardName)
            //    characterDrawName = characterName;

            //if (terms != null && terms.Length >= 1 && terms.Contains("all"))
            //    all = true;
            //if (terms != null && terms.Length >= 1 && terms.Contains("redraw"))
            //    redraw = true;
            //if (terms != null && terms.Length >= 1 && (terms.Contains("s") || terms.Contains("secret")))
            //    secretDraw = true;

            //List<int> discardsTemp = Utils.GetAllNumbersFromInputs(terms);
            //List<int> discards = new List<int>();

            ////decrease all the numbers by 1 to match array indexes, rather than the card position for a player
            //if (discardsTemp.Count > 0)
            //{
            //    foreach (int i in discardsTemp)
            //    {
            //        discards.Add(i - 1);
            //    }
            //}

            //string cardsS = "";
            //if (discards.Count > 1)
            //    cardsS = "s";

            //DeckType deckType = commandController.GetDeckTypeFromCommandTerms(terms);

            //string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType);
            //string cardDrawingCharacterString = Utils.GetCharacterStringFromSpecialName(characterDrawName);

            //int numberDiscards = 0;
            //string messageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + cardsS + " discarded from play:[/i] " + bot.DiceBot.DiscardCardsFromPlay(discards, all, channel, deckType, characterDrawName, out numberDiscards);
            //if (redraw)
            //{
            //    string trueDraw = "";
            //    messageOutput += "\n [i]Redrawn:[/i] " + bot.DiceBot.DrawCards(numberDiscards, false, true, channel, deckType, characterDrawName, secretDraw, out trueDraw);
            //}

            //if (secretDraw && !(characterDrawName == DiceBot.DealerName || characterDrawName == DiceBot.BurnCardsName))
            //{
            //    bot.SendPrivateMessage(messageOutput, characterName);
            //}

            //if (secretDraw)
            //{
            //    string redrawSecretString = redraw ? " (and redrew)" : "";
            //    string newMessageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + cardsS + " discarded from play: (secret)" + redrawSecretString + "[/i] ";
            //    bot.SendMessageInChannel(newMessageOutput, channel);
            //}
            //else
            //{
            //    bot.SendMessageInChannel(messageOutput, channel);
            //}
        }
    }
}
