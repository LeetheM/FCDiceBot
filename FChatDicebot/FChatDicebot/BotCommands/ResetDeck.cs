using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands
{
    public class ResetDeck : ChatBotCommand
    {
        public ResetDeck()
        {
            Name = "resetdeck";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelDecks;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            bool jokers = false;
            if (terms != null && terms.Length >= 1 && terms.Contains("j"))
                jokers = true;

            DeckType deckType = commandController.GetDeckTypeFromCommandTerms(terms);

            string customDeckName = Utils.GetCustomDeckName(characterName);
            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType, customDeckName);

            bot.DiceBot.ResetDeck(jokers, channel, deckType, customDeckName);
            bot.SendMessageInChannel("[i]" + deckTypeString + "Channel deck reset." + (jokers ? " (contains jokers)" : "") + "[/i]", channel);
        }
    }
}
