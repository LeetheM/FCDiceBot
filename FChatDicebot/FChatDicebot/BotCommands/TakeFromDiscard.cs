using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands
{
    public class TakeFromDiscard : ChatBotCommand
    {
        public TakeFromDiscard()
        {
            Name = "takefromdiscard";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelDecks;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            MoveCards.Run(bot, commandController, rawTerms, terms, characterName, channel, command, CardPileId.Discard, CardPileId.Hand);//, CardMoveType.ToHandFromDiscard);

            //bool all = false;
            //string characterDrawName = commandController.GetCharacterDrawNameFromCommandTerms(characterName, terms);
            //if (terms != null && terms.Length >= 1 && terms.Contains("all"))
            //    all = true;

            //List<int> drawsFromDiscardTemp = Utils.GetAllNumbersFromInputs(terms);
            //List<int> drawsFromDiscard = new List<int>();

            ////decrease all the numbers by 1 to match array indexes, rather than the card position for a player
            //if (drawsFromDiscardTemp.Count > 0)
            //{
            //    foreach (int i in drawsFromDiscardTemp)
            //    {
            //        drawsFromDiscard.Add(i - 1);
            //    }
            //}

            //string cardsS = drawsFromDiscard.Count > 1 ? "s" : "";

            //DeckType deckType = commandController.GetDeckTypeFromCommandTerms(terms);

            //string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType);
            //string cardDrawingCharacterString = Utils.GetCharacterStringFromSpecialName(characterDrawName);

            //string messageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + cardsS + " taken from discard:[/i] " + bot.DiceBot.TakeCardsFromDiscard(drawsFromDiscard, all, channel, deckType, characterDrawName);

            //bot.SendMessageInChannel(messageOutput, channel);
        }
    }
}
