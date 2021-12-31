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
    public class UpdateSetting : ChatBotCommand
    {
        public UpdateSetting()
        {
            Name = "updatesetting";
            RequireBotAdmin = false;
            RequireChannelAdmin = true;
            RequireChannel = true;
            LockCategory = CommandLockCategory.SavedChannels;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            SettingType changed = SettingType.NONE;

            ChannelSettings thisChannel = bot.GetChannelSettings(channel);

            bool setValue = false;

            if (terms != null && terms.Length >= 1)
            {
                if (terms.Contains("useeicons"))
                    changed = SettingType.UseEicons;
                if (terms.Contains("greetnewusers"))
                    changed = SettingType.GreetNewUsers;
                if (terms.Contains("allowtablerolls"))
                    changed = SettingType.AllowTableRolls;
                if (terms.Contains("allowcustomtablerolls"))
                    changed = SettingType.AllowCustomTableRolls;
                if (terms.Contains("allowtableinfo"))
                    changed = SettingType.AllowTableInfo;
                if (terms.Contains("allowchips"))
                    changed = SettingType.AllowChips;
                if (terms.Contains("allowgames"))
                    changed = SettingType.AllowGames;
                if (terms.Contains("usevcaccount"))
                    changed = SettingType.UseVcAccountForChips;
                if (terms.Contains("startupchannel"))
                    changed = SettingType.StartupChannel;
                if (terms.Contains("startwith500chips"))
                    changed = SettingType.StartWith500Chips;
                if (terms.Contains("onlyopaddchips"))
                    changed = SettingType.OnlyOpAddChips;
                if (terms.Contains("onlyopbotcommands"))
                    changed = SettingType.OnlyOpBotCommands;
                if (terms.Contains("onlyopdeckcontrols"))
                    changed = SettingType.OnlyOpDeckControls;
                if (terms.Contains("onlyoptablecommands"))
                    changed = SettingType.OnlyOpTableCommands;

                if (terms.Contains("on") || terms.Contains("true"))
                    setValue = true;
                if (terms.Contains("off") || terms.Contains("false"))
                    setValue = false;
            }

            switch (changed)
            {
                case SettingType.NONE:
                    break;
                case SettingType.UseEicons:
                    thisChannel.UseEicons = setValue;
                    break;
                case SettingType.GreetNewUsers:
                    thisChannel.GreetNewUsers = setValue;
                    break;
                case SettingType.AllowTableRolls:
                    thisChannel.AllowTableRolls = setValue;
                    break;
                case SettingType.AllowCustomTableRolls:
                    thisChannel.AllowCustomTableRolls = setValue;
                    break;
                case SettingType.AllowTableInfo:
                    thisChannel.AllowTableInfo = setValue;
                    break;
                case SettingType.AllowChips:
                    thisChannel.AllowChips = setValue;
                    break;
                case SettingType.AllowGames:
                    thisChannel.AllowGames = setValue;
                    break;
                case SettingType.UseVcAccountForChips:
                    thisChannel.UseVcAccountForChips = setValue;
                    break;
                case SettingType.StartupChannel:
                    thisChannel.StartupChannel = setValue;
                    break;
                case SettingType.StartWith500Chips:
                    thisChannel.StartWith500Chips = setValue;
                    break;
                case SettingType.OnlyOpAddChips:
                    {
                        if(thisChannel.ChipsClearance != ChipsClearanceLevel.DicebotAdmin)
                        {
                            if(setValue)
                            {
                                thisChannel.ChipsClearance = ChipsClearanceLevel.ChannelOp;
                            }
                            else
                            {
                                thisChannel.ChipsClearance = ChipsClearanceLevel.NONE;
                            }
                        }
                    }
                    break;
                case SettingType.OnlyOpBotCommands:
                    thisChannel.OnlyChannelOpsCanUseAnyBotCommands = setValue;
                    break;
                case SettingType.OnlyOpDeckControls:
                    thisChannel.OnlyChannelOpsCanUseDeckControls = setValue;
                    break;
                case SettingType.OnlyOpTableCommands:
                    thisChannel.OnlyChannelOpsCanUseTableCommands = setValue;
                    break;
            }

            if (changed == SettingType.NONE)
            {
                string output = "Setting not found. Be sure to specify which setting to change, followed by 'true' or 'false'. Settings use the same name displayed in the !viewsettings command.";

                bot.SendMessageInChannel(output, channel);
            }
            else
            {
                Utils.WriteToFileAsData(bot.SavedChannelSettings, Utils.GetTotalFileName(BotMain.FileFolder, BotMain.ChannelSettingsFileName));

                string output = "(Channel setting updated) " + Utils.GetCharacterUserTags(characterName) + " set " + changed + " to " + setValue;

                if (changed == SettingType.OnlyOpAddChips && thisChannel.ChipsClearance == ChipsClearanceLevel.DicebotAdmin)
                {
                    output = "(" + changed + " Channel setting cannot be updated for this channel) ";
                }

                bot.SendMessageInChannel(output, channel);
            }
        }
    }

    public enum SettingType
    {
        NONE,
        UseEicons,
        GreetNewUsers,
        AllowTableRolls,
        AllowCustomTableRolls,
        AllowTableInfo,
        AllowChips,
        AllowGames,
        UseVcAccountForChips,
        StartupChannel,
        OnlyOpAddChips,
        OnlyOpBotCommands,
        OnlyOpDeckControls,
        OnlyOpTableCommands,
        StartWith500Chips
    }
}
        ////todo: apply settings to code
        //public bool UseEicons;
        //public bool GreetNewUsers;
        //public bool UseVcAccountForChips;
        //OnlyOpBotCommands,
        //OnlyOpDeckControls,
        //OnlyOpTableCommands,
