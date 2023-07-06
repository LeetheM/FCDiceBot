using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands
{
    public class TestViewDeck : ChatBotCommand
    {
        public TestViewDeck()
        {
            Name = "testviewdeck";
            RequireBotAdmin = true;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.NONE;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            DeckType deckType = commandController.GetDeckTypeFromCommandTerms(terms);

            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType, characterName);
            
            string customDeckName = Utils.GetCustomDeckName(characterName);
            
            string deckList = bot.DiceBot.ViewEntireDeck(channel, deckType, customDeckName);
            bot.SendMessageInChannel("[i]" + deckTypeString + "Channel deck list. :[/i]" + deckList, channel);
        }
    }
}
