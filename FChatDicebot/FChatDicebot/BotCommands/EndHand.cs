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
            bool reveal = false;
            if (terms != null && terms.Contains("reveal"))
                reveal = true;

            string customDeckString = Utils.GetCustomDeckName(characterName);
            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType, customDeckString);
            string rtn = bot.DiceBot.EndHand(channel, reveal, deckType);
            bot.SendMessageInChannel("[i]" + rtn + "[/i]", channel);
        }
    }
}
