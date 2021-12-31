using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands
{
    public class DeckList : ChatBotCommand
    {
        public DeckList()
        {
            Name = "decklist";
            RequireBotAdmin = true;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelDecks;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            DeckType deckType = commandController.GetDeckTypeFromCommandTerms(terms);

            string customDeckName = Utils.GetCustomDeckName(characterName);
            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType, customDeckName);

            Deck a = bot.DiceBot.GetDeck(channel, deckType, customDeckName);
            string sendString = "";
            if(a != null)
                sendString = "[i]" + deckTypeString + "Channel deck contents: [/i]" + a.ToString();
            else
                sendString = "[i]Error: " + deckTypeString + " deck not found[/i]";

            bot.SendMessageInChannel(sendString, channel);
        }
    }
}
