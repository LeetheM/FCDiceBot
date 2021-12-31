using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.SavedData;
using Newtonsoft.Json;
using FChatDicebot.DiceFunctions;

namespace FChatDicebot.BotCommands
{
    public class Register : ChatBotCommand
    {
        public Register()
        {
            Name = "register";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.ChannelScores;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            ChannelSettings thisChannel = bot.GetChannelSettings(channel);

            if (thisChannel.AllowChips)
            {
                string messageString = "";

                ChipPile existing = bot.DiceBot.GetChipPile(characterName, channel, false);
                if(existing != null)
                {
                    messageString = Utils.GetCharacterUserTags(characterName) + " is already registered.";
                }
                else
                {
                    messageString = Utils.GetCharacterUserTags(characterName) + " was registered for a chips pile.";

                    if(thisChannel.StartWith500Chips)
                        messageString += "\n[b]500 chips[/b] were given to " + Utils.GetCharacterUserTags(characterName) + " to start.";

                    bot.DiceBot.AddChips(characterName, channel, 0, false);

                    commandController.SaveChipsToDisk();
                }

                bot.SendMessageInChannel(messageString, channel);
            }
            else
            {
                bot.SendMessageInChannel(Name + " is currently not allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.", channel);
            }
        }
    }
}
