using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands
{
    public class ShowHand : ChatBotCommand
    {
        public ShowHand()
        {
            Name = "showhand";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelDecks;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            string characterDrawName = commandController.GetCharacterDrawNameFromCommandTerms(characterName, terms);

            DeckType deckType = commandController.GetDeckTypeFromCommandTerms(terms);

            string customDeckName = Utils.GetCustomDeckName(characterName);
            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType, customDeckName);
            Hand h = bot.DiceBot.GetHand(channel, deckType, characterDrawName);

            string displayName = characterDrawName;
            if (displayName.Contains(DiceBot.PlaySuffix))
                displayName = displayName.Replace(DiceBot.PlaySuffix, "");

            string outputString = "[i]" + deckTypeString + "Showing [user]" + displayName + "[/user]'s " + h.GetCollectionName() + ": [/i]" + h.ToString();
            if (characterDrawName == DiceBot.BurnCardsName)
                outputString = "[i]" + deckTypeString + "Showing burned cards: [/i]" + h.ToString();
            else if (characterDrawName == DiceBot.DealerName)
                outputString = "[i]" + deckTypeString + "Showing the dealer's hand: [/i]" + h.ToString();
            else if (characterDrawName == DiceBot.DiscardName)
                outputString = "[i]" + deckTypeString + "Showing discarded cards: [/i]" + h.ToString();

            bot.SendMessageInChannel(outputString, channel);
        }
    }
}
