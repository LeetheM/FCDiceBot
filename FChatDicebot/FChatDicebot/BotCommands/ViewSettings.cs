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
                "[b]AllowCustomTableRolls:[/b] {4}, [b]GreetNewUsers:[/b] {5}, [b]AllowChips:[/b] {6}, [b]AllowSlots[/b]: {7}\n" +
                "[b]Add Chips Restriction:[/b] {8} (OnlyOpAddChips), [b]StartingChips[/b]: {9}, [b]RemoveSlotsCooldown[/b]: {10}, [b]DefaultSlotsFruit:[/b] {11}, [b]SlotsMultiplierLimit[/b] {12}\n" +
                "[b]AllowGames:[/b] {13}, [b]AllowChess:[/b] {14}, [b]AllowChessEicons:[/b] {15}, [b]CommandPrefix:[/b] {16}, [b]RemoveLuckForecastCooldown:[/b] {17}",
                thisChannel.Name,
                thisChannel.StartupChannel, thisChannel.AllowTableInfo, thisChannel.AllowTableRolls, 
                thisChannel.AllowCustomTableRolls, thisChannel.GreetNewUsers, thisChannel.AllowChips, thisChannel.AllowSlots,
                thisChannel.ChipsClearance.ToString(), thisChannel.StartingChips, thisChannel.RemoveSlotsCooldown, thisChannel.DefaultSlotsFruit, thisChannel.SlotsMultiplierLimit,
                thisChannel.AllowGames, thisChannel.AllowChess, thisChannel.AllowChessEicons, thisChannel.CommandPrefix.ToString(), thisChannel.RemoveLuckForecastCooldown);

            bot.SendMessageInChannel(allSettings, channel);
        }
    }
}