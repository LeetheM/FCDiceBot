using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands
{
    public class DrawCard : ChatBotCommand
    {
        public DrawCard()
        {
            Name = "drawcard";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelDecks;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            CardCommandOptions options = new CardCommandOptions(commandController, terms, characterName);

            int numberDrawn = Utils.GetNumberFromInputs(terms);
            if (numberDrawn > 1)
                options.cardsS = "s";

            string customDeckName = Utils.GetCustomDeckName(characterName);

            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(options.deckType, customDeckName);
            string cardDrawingCharacterString = Utils.GetCharacterStringFromSpecialName(options.characterDrawName);

            string trueDraw = "";
            string drawOutput = bot.DiceBot.DrawCards(numberDrawn, options.jokers, options.deckDraw, channel, options.deckType, options.characterDrawName, options.secretDraw, out trueDraw);
            string channelMessageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + options.cardsS + " drawn:[/i] " + drawOutput;
            if (options.secretDraw && !(options.characterDrawName == DiceBot.DealerName || options.characterDrawName == DiceBot.BurnCardsName || options.characterDrawName == DiceBot.DiscardName))
            {
                string playerMessageOutput = "[i]" + cardDrawingCharacterString + ": " + deckTypeString + "Card" + options.cardsS + " drawn:[/i] " + trueDraw;
                bot.SendPrivateMessage(playerMessageOutput, characterName);
            }

            bot.SendMessageInChannel(channelMessageOutput, channel);
        }
    }
}
