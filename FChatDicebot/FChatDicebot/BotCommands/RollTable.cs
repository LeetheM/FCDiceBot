using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.SavedData;

namespace FChatDicebot.BotCommands
{
    public class RollTable : ChatBotCommand
    {
        public RollTable()
        {
            Name = "rolltable";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.SavedTables;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            ChannelSettings thisChannel = bot.GetChannelSettings(channel);

            if (thisChannel.AllowTableRolls)
            {
                bool includeLabel = true;
                bool secondaryRolls = true;

                if (terms != null && terms.Length >= 1 && terms.Contains("nolabel"))
                    includeLabel = false;
                if (terms != null && terms.Length >= 1 && terms.Contains("nosecondary"))
                    secondaryRolls = false;

                int rollModifier = commandController.GetRollModifierFromCommandTerms(terms);
                string tableName = commandController.GetTableNameFromCommandTerms(terms);

                SavedRollTable savedTable = Utils.GetTableFromId(bot.SavedTables, tableName);

                if(savedTable.DefaultTable || thisChannel.AllowCustomTableRolls)
                {
                    string sendMessage = bot.DiceBot.GetRollTableResult(bot.SavedTables, tableName, characterName, channel, rollModifier, includeLabel, secondaryRolls, 3);

                    bot.SendMessageInChannel(sendMessage, channel);
                }
                else
                {
                    bot.SendMessageInChannel("Only default talbes are allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.", channel);
                }
            }
            else
            {
                bot.SendMessageInChannel(Name + " is currently not allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.", channel);
            }
        }
    }
}
