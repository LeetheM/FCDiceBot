using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.DiceFunctions;
using FChatDicebot.SavedData;

namespace FChatDicebot.BotCommands
{
    //NOTE: admin only used to manually edit settings for every channel at once
    public class UpdateSettingAll : ChatBotCommand
    {
        public UpdateSettingAll()
        {
            Name = "updatesettingall";
            RequireBotAdmin = true;
            RequireChannelAdmin = true;
            RequireChannel = true;
            LockCategory = CommandLockCategory.SavedChannels;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            foreach (var settings in bot.SavedChannelSettings)
            {
                settings.SlotsMultiplierLimit = 1000;
                if (settings.StartWith500Chips)
                    settings.StartingChips = 500;
                else
                    settings.StartingChips = 0;
            }

            commandController.SaveChannelSettingsToDisk();

            string output = "(All Channels setting updated) set slots multiplier to 1000, updated startingchips";
            
            bot.SendMessageInChannel(output, channel);
        }
    }

}