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
    public class ClaimPot : ChatBotCommand
    {
        public ClaimPot()
        {
            Name = "claimpot";
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
                bool half = false;
                bool third = false;

                if (terms != null && terms.Length >= 1 && terms.Contains("half"))
                    half = true;

                if (terms != null && terms.Length >= 1 && terms.Contains("third"))
                    third = true;

                string messageString = bot.DiceBot.ClaimPot(characterName, channel, half, third);

                commandController.SaveChipsToDisk();

                bot.SendMessageInChannel(messageString, channel);
            }
            else
            {
                bot.SendMessageInChannel(Name + " is currently not allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.", channel);
            }
        }
    }
}
