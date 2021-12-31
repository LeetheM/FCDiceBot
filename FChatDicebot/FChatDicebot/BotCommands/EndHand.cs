using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands
{
    public class EndHand : ChatBotCommand
    {
        public EndHand()
        {
            Name = "endhand";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelDecks;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            DeckType deckType = commandController.GetDeckTypeFromCommandTerms(terms);

            string customDeckString = Utils.GetCustomDeckName(characterName);
            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType, customDeckString);
            bot.DiceBot.EndHand(channel, deckType);
            bot.SendMessageInChannel("[i]" + deckTypeString + "All hands have been emptied.[/i]", channel);
        }
    }
}
