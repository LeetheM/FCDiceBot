using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands.Base
{
    public class MoveCards
    {
        public static void Run(BotMain bot, BotCommandController commandController, 
            string[] rawTerms, string[] terms, string characterName, string channel, 
            UserGeneratedCommand command, CardMoveType moveType)
        {
            CardCommandOptions options = new CardCommandOptions(commandController, terms, characterName);
            if (!(moveType == CardMoveType.DiscardCard || moveType == CardMoveType.PlayCard))
                options.redraw = false;

            string customDeckString = Utils.GetCustomDeckName(characterName);
            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(options.deckType, customDeckString);
            string cardDrawingCharacterString = Utils.GetCharacterStringFromSpecialName(options.characterDrawName);

            int numberDiscards = 0;

            string actionString = "";
            switch(moveType)
            {
                case CardMoveType.DiscardCard:
                    actionString = bot.DiceBot.DiscardCards(options.moveCardsList, options.all, channel, options.deckType, options.characterDrawName, out numberDiscards);
                    break;
                case CardMoveType.PlayCard:
                    actionString = bot.DiceBot.PlayCards(options.moveCardsList, options.all, channel, options.deckType, options.characterDrawName, out numberDiscards);
                    break;
                case CardMoveType.ToHandFromDiscard:
                    actionString = bot.DiceBot.TakeCardsFromDiscard(options.moveCardsList, options.all, channel, options.deckType, options.characterDrawName, out numberDiscards);
                    break;
                case CardMoveType.ToPlayFromDiscard:
                    actionString = bot.DiceBot.PlayCardsFromDiscard(options.moveCardsList, options.all, channel, options.deckType, options.characterDrawName, out numberDiscards);
                    break;
                case CardMoveType.ToHandFromPlay:
                    actionString = bot.DiceBot.TakeCardsFromPlay(options.moveCardsList, options.all, channel, options.deckType, options.characterDrawName, out numberDiscards);
                    break;
                case CardMoveType.ToDiscardFromPlay:
                    actionString = bot.DiceBot.DiscardCardsFromPlay(options.moveCardsList, options.all, channel, options.deckType, options.characterDrawName, out numberDiscards);
                    break;
            }

            if(numberDiscards > 1 || numberDiscards == 0)
            {
                options.cardsS = "s";
            }
            string messageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + options.cardsS + 
                " " + Utils.GetCardMoveTypeString(moveType) + "[/i] " + actionString;
            string trueDraw = "";
            string privateMessageDraw = "";
            if (options.redraw)
            {
                messageOutput += "\n [i]Redrawn:[/i] " + bot.DiceBot.DrawCards(numberDiscards, options.jokers, options.deckDraw, channel, options.deckType, options.characterDrawName, options.secretDraw, out trueDraw);
                privateMessageDraw = "[i]Redrawn:[/i] " + trueDraw;
            }

            if (options.secretDraw && options.redraw && !(options.characterDrawName == DiceBot.DealerName || options.characterDrawName == DiceBot.BurnCardsName || options.characterDrawName == DiceBot.DiscardName))
            {
                bot.SendPrivateMessage(privateMessageDraw, characterName);
            }

            if (options.secretDraw)
            {
                string redrawSecretString = options.redraw ? " (and redrew)" : "";
                string newMessageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + numberDiscards + " Card" + options.cardsS + " " + Utils.GetCardMoveTypeString(moveType) + " (secret)" + redrawSecretString + "[/i] ";
                bot.SendMessageInChannel(newMessageOutput, channel);
            }
            else
            {
                bot.SendMessageInChannel(messageOutput, channel);
            }
        }
    }

    public class CardCommandOptions
    {
        public string characterDrawName;
        public DeckType deckType;

        public bool secretDraw;

        public bool all;

        public bool redraw;
        public bool jokers;
        public bool deckDraw;

        public List<int> moveCardsList = new List<int>();
        public string cardsS;

        public CardCommandOptions(BotCommandController commandController, string[] terms, string characterName)
        {
            deckDraw = true;
            all = false;
            redraw = false;
            secretDraw = false;
            characterDrawName = commandController.GetCharacterDrawNameFromCommandTerms(characterName, terms);
            if (characterDrawName == DiceBot.DiscardName)
                characterDrawName = characterName;

            if (terms != null && terms.Length >= 1 && terms.Contains("all"))
                all = true;
            if (terms != null && terms.Length >= 1 && terms.Contains("redraw"))
                redraw = true;
            if (terms != null && terms.Length >= 1 && (terms.Contains("s") || terms.Contains("secret")))
                secretDraw = true;
            if (terms != null && terms.Length >= 1 && terms.Contains("j"))
                jokers = true;
            if (terms != null && terms.Length >= 1 && terms.Contains("nodeck"))
                deckDraw = false;

            List<int> discardsTemp = Utils.GetAllNumbersFromInputs(terms);

            //decrease all the numbers by 1 to match array indexes, rather than the card position for a player
            if (discardsTemp.Count > 0)
            {
                foreach (int i in discardsTemp)
                {
                    moveCardsList.Add(i - 1);
                }
            }

            if (moveCardsList.Count > 1)
                cardsS = "s";

            deckType = commandController.GetDeckTypeFromCommandTerms(terms);

        }

    }
}
