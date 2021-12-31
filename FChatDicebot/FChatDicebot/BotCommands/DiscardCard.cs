using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands
{
    public class DiscardCard : ChatBotCommand
    {
        public DiscardCard()
        {
            Name = "discardcard";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelDecks;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            MoveCards.Run(bot, commandController, rawTerms, terms, characterName, channel, command, CardMoveType.DiscardCard);

            //CardCommandOptions options = new CardCommandOptions(bot, commandController, rawTerms, terms, characterName, channel, command);
            //string deckTypeString = Utils.GetDeckTypeStringHidePlaying(options.deckType);
            //string cardDrawingCharacterString = Utils.GetCharacterStringFromSpecialName(options.characterDrawName);

            //int numberDiscards = 0;
            //string messageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + options.cardsS + " discarded:[/i] " + bot.DiceBot.DiscardCards(options.moveCardsList, options.all, channel, options.deckType, options.characterDrawName, out numberDiscards);
            //string trueDraw = "";
            //string privateMessageDraw = "";
            //if (options.redraw)
            //{
            //    messageOutput += "\n [i]Redrawn:[/i] " + bot.DiceBot.DrawCards(numberDiscards, false, true, channel, options.deckType, options.characterDrawName, options.secretDraw, out trueDraw);
            //    privateMessageDraw = "[i]Redrawn:[/i] " + trueDraw;
            //}

            //if (options.secretDraw && options.redraw && !(options.characterDrawName == DiceBot.DealerName || options.characterDrawName == DiceBot.BurnCardsName || options.characterDrawName == DiceBot.DiscardName))
            //{
            //    bot.SendPrivateMessage(privateMessageDraw, characterName);
            //}

            //if (options.secretDraw)
            //{
            //    string redrawSecretString = options.redraw ? " (and redrew)" : "";
            //    string newMessageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + options.cardsS + " discarded: (secret)" + redrawSecretString + "[/i] ";
            //    bot.SendMessageInChannel(newMessageOutput, channel);
            //}
            //else
            //{
            //    bot.SendMessageInChannel(messageOutput, channel);
            //}
        }
    }
}
