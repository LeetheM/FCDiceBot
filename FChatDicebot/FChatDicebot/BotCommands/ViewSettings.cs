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
    public class ViewSettings : ChatBotCommand
    {
        public ViewSettings()
        {
            Name = "viewsettings";
            RequireBotAdmin = false;
            RequireChannelAdmin = true;
            RequireChannel = true;
            LockCategory = CommandLockCategory.SavedChannels;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            ChannelSettings thisChannel = bot.GetChannelSettings(channel);

            string allSettings = string.Format("{0} [b]Channel Settings[/b]: \n" +
                "[b]StartupChannel:[/b] {1}, [b]AllowTableInfo:[/b] {2}, [b]AllowTableRolls:[/b] {3},\n" + 
                "[b]AllowCustomTableRolls:[/b] {4}, [b]GreetNewUsers:[/b] {5}, [b]AllowChips:[/b] {6} \n" +
                "[b]Add Chips Restriction:[/b] {7} (OnlyOpAddChips), [b]StartWith500Chips[/b]: {8} \n" + 
                "[b]AllowGames:[/b] {9}",
                thisChannel.Name,
                thisChannel.StartupChannel, thisChannel.AllowTableInfo, thisChannel.AllowTableRolls, 
                thisChannel.AllowCustomTableRolls, thisChannel.GreetNewUsers, thisChannel.AllowChips,
                thisChannel.ChipsClearance.ToString(), thisChannel.StartWith500Chips,
                thisChannel.AllowGames);

            bot.SendMessageInChannel(allSettings, channel);
            
        }
    }
}