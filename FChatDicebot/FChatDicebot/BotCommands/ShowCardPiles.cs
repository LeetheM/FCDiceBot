using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands
{
    public class ShowCardPiles : ChatBotCommand
    {
        public ShowCardPiles()
        {
            Name = "showcardpiles";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelDecks;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            bool allPiles = false;
            bool revealHand = false;
            bool secretOutput = false;
            string characterDrawName = commandController.GetCharacterDrawNameFromCommandTerms(characterName, terms);

            if (terms.Contains("reveal"))
                revealHand = true;

            if (terms.Contains("all"))
                allPiles = true;

            if (terms.Contains("s") || terms.Contains("secret"))
                secretOutput = true;

            DeckType deckType = commandController.GetDeckTypeFromCommandTerms(terms);

            string customDeckName = Utils.GetCustomDeckName(characterName);
            string deckTypeString = Utils.GetDeckTypeStringHidePlaying(deckType, customDeckName);
            Hand h = bot.DiceBot.GetHand(channel, deckType, characterName);

            string displayName = characterDrawName;
            if (displayName.Contains(DiceBot.PlaySuffix))
                displayName = displayName.Replace(DiceBot.PlaySuffix, "");
            if (displayName.Contains(DiceBot.HiddenPlaySuffix))
                displayName = displayName.Replace(DiceBot.HiddenPlaySuffix, "");

            string outputString = "[i]Card piles for " + deckTypeString;

            bool showHandSize = !revealHand;
            if (secretOutput)
                showHandSize = false;
            outputString += "\n[user]" + displayName + "[/user]'s " + h.GetCollectionName() + ": [/i]" + h.ToString(showHandSize);

            Hand hiddenplay = bot.DiceBot.GetHand(channel, deckType, characterName + DiceBot.HiddenPlaySuffix);

            if(!secretOutput && h != null)
            {
                string privateOutput = "[i]" + deckTypeString + " " + h.GetCollectionName() + ": [/i]" + h.ToString(false);
                if(hiddenplay != null && hiddenplay.CardsCount() > 0)
                {
                    privateOutput += "\n[i]" + deckTypeString + " " + h.GetCollectionName() + ": [/i]" + hiddenplay.ToString(false);
                }
                bot.SendPrivateMessage(privateOutput, characterName);
            }

            Hand play = bot.DiceBot.GetHand(channel, deckType, characterName + DiceBot.PlaySuffix);
            outputString += "\n[i]cards in play: [/i]" + play.ToString();
            if(hiddenplay != null && (hiddenplay.CardsCount() > 0 || allPiles))
            {
                outputString += "\n[i]hidden cards in play: [/i]" + hiddenplay.ToString(showHandSize);
            }
            Hand discarded = bot.DiceBot.GetHand(channel, deckType, DiceBot.DiscardPlayerAlias);
            if(discarded != null && (discarded.CardsCount() > 0 || allPiles))
            {
                outputString += "\n[i]discarded cards: [/i]" + discarded.ToString();
            }

            if (characterDrawName == DiceBot.BurnCardsPlayerAlias || allPiles)
            {
                Hand hNew = bot.DiceBot.GetHand(channel, deckType, DiceBot.BurnCardsPlayerAlias);
                outputString += "\n[i]burned cards: [/i]" + hNew.ToString();
            }
            if (characterDrawName == DiceBot.DealerPlayerAlias || allPiles)
            {
                Hand hNew = bot.DiceBot.GetHand(channel, deckType, DiceBot.DealerPlayerAlias);
                outputString += "\n[i]dealer's hand: [/i]" + hNew.ToString();
            }

            if(secretOutput)
            {
                bot.SendPrivateMessage(outputString, characterName);
                bot.SendMessageInChannel("Sent information for card piles to " + Utils.GetCharacterUserTags(characterName), channel);
            }
            else
            {
                bot.SendMessageInChannel(outputString, channel);
            }
        }
    }
}
