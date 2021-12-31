using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.SavedData;
using Newtonsoft.Json;

namespace FChatDicebot.BotCommands
{
    public class DeleteCustomDeck : ChatBotCommand
    {
        public DeleteCustomDeck()
        {
            Name = "deletecustomdeck";
            RequireBotAdmin = false;
            RequireChannelAdmin = true;
            RequireChannel = true;
            LockCategory = CommandLockCategory.SavedTables;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            string customDeckName = Utils.GetCustomDeckName(characterName);

            SavedDeck deleteDeck = Utils.GetDeckFromId(bot.SavedDecks, customDeckName);

            string sendMessage = "No decks found for [user]" + characterName + "[/user]";
            if (deleteDeck != null)
            {
                if (characterName == deleteDeck.OriginCharacter)
                {
                    bot.SavedDecks.Remove(deleteDeck);

                    sendMessage = "[b]" + deleteDeck.DeckId + "[/b] deleted by [user]" + characterName + "[/user]";

                    Utils.WriteToFileAsData(bot.SavedDecks, Utils.GetTotalFileName(BotMain.FileFolder, BotMain.SavedDecksFileName));
                }
                else
                {
                    sendMessage = "Only " + deleteDeck.OriginCharacter + " can delete their own saved deck.";
                }
            }

            bot.SendMessageInChannel(sendMessage, channel);
        }
    }
}
