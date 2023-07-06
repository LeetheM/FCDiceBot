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
            int setNumber = 0;
            char setChar = '!';

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
                if (terms.Contains("allowslots"))
                    changed = SettingType.AllowSlots;
                if (terms.Contains("allowgames"))
                    changed = SettingType.AllowGames;
                if (terms.Contains("allowchesseicons"))
                    changed = SettingType.AllowChessEicons;
                if (terms.Contains("allowchess"))
                    changed = SettingType.AllowChess;
                if (terms.Contains("usevcaccount"))
                    changed = SettingType.UseVcAccountForChips;
                if (terms.Contains("startupchannel"))
                    changed = SettingType.StartupChannel;
                if (terms.Contains("startwith500chips"))
                    changed = SettingType.StartWith500Chips;
                if (terms.Contains("startingchips"))
                    changed = SettingType.StartingChips;
                if (terms.Contains("removeslotscooldown"))
                    changed = SettingType.RemoveSlotsCooldown;
                if (terms.Contains("removeluckforecastcooldown"))
                    changed = SettingType.RemoveLuckForecastCooldown;
                if (terms.Contains("defaultslotsfruit"))
                    changed = SettingType.DefaultSlotsFruit;
                if (terms.Contains("onlyopaddchips"))
                    changed = SettingType.OnlyOpAddChips;
                if (terms.Contains("slotsmultiplierlimit"))
                    changed = SettingType.SlotsMultiplierLimit;
                if (terms.Contains("onlyopbotcommands"))
                    changed = SettingType.OnlyOpBotCommands;
                if (terms.Contains("onlyopdeckcontrols"))
                    changed = SettingType.OnlyOpDeckControls;
                if (terms.Contains("onlyoptablecommands"))
                    changed = SettingType.OnlyOpTableCommands;
                if (terms.Contains("commandprefix"))
                    changed = SettingType.CommandPrefix;

                if (terms.Contains("on") || terms.Contains("true"))
                    setValue = true;
                else if (terms.Contains("off") || terms.Contains("false"))
                    setValue = false;
                else if(terms.Length > 1)
                {
                    int numberFound = Utils.GetNumberFromInputs(terms);
                    setNumber = numberFound;
                    if (numberFound < 0)
                        numberFound = 0;

                    var lastTerm = terms[terms.Length - 1];
                    if(lastTerm.Length == 1)
                        setChar = lastTerm.ToCharArray()[0];
                }
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
                case SettingType.AllowSlots:
                    thisChannel.AllowSlots = setValue;
                    break;
                case SettingType.AllowGames:
                    thisChannel.AllowGames = setValue;
                    break;
                case SettingType.AllowChess:
                    thisChannel.AllowChess = setValue;
                    break;
                case SettingType.AllowChessEicons:
                    thisChannel.AllowChessEicons = setValue;
                    break;
                case SettingType.UseVcAccountForChips:
                    thisChannel.UseVcAccountForChips = setValue;
                    break;
                case SettingType.StartupChannel:
                    thisChannel.StartupChannel = setValue;
                    break;
                case SettingType.RemoveSlotsCooldown:
                    thisChannel.RemoveSlotsCooldown = setValue;
                    break;
                case SettingType.RemoveLuckForecastCooldown:
                    thisChannel.RemoveLuckForecastCooldown = setValue;
                    break;
                case SettingType.DefaultSlotsFruit:
                    thisChannel.DefaultSlotsFruit = setValue;
                    break;
                case SettingType.StartWith500Chips:
                    thisChannel.StartWith500Chips = setValue;
                    break;
                case SettingType.StartingChips:
                    thisChannel.StartingChips = setNumber;
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
                case SettingType.SlotsMultiplierLimit:
                    thisChannel.SlotsMultiplierLimit = setNumber;
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
                case SettingType.CommandPrefix:
                    thisChannel.CommandPrefix = setChar;
                    break;
            }

            if (changed == SettingType.NONE)
            {
                string output = "Setting not found. Be sure to specify which setting to change, followed by 'true' or 'false'. Settings use the same name displayed in the !viewsettings command.";

                bot.SendMessageInChannel(output, channel);
            }
            else
            {
                commandController.SaveChannelSettingsToDisk();

                string output = "(Channel setting updated) " + Utils.GetCharacterUserTags(characterName) + " set " + changed + " to ";
                if (changed == SettingType.CommandPrefix)
                    output += setChar.ToString();
                else if (changed == SettingType.SlotsMultiplierLimit || changed == SettingType.StartingChips)
                    output += setNumber.ToString();
                else
                    output += setValue.ToString();

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
        AllowSlots,
        AllowGames,
        UseVcAccountForChips,
        StartupChannel,
        OnlyOpAddChips,
        OnlyOpBotCommands,
        OnlyOpDeckControls,
        OnlyOpTableCommands,
        StartWith500Chips,
        RemoveSlotsCooldown,
        RemoveLuckForecastCooldown,
        DefaultSlotsFruit,
        CommandPrefix,
        AllowChessEicons,
        AllowChess,
        SlotsMultiplierLimit,
        StartingChips
    }
}
        ////todo: apply settings to code
        //public bool UseEicons;
        //public bool GreetNewUsers;
        //public bool UseVcAccountForChips;
        //OnlyOpBotCommands,
        //OnlyOpDeckControls,
        //OnlyOpTableCommands,
