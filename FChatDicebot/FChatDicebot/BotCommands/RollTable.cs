using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChatDicebot.BotCommands.Base;
using FChatDicebot.SavedData;
using FChatDicebot.DiceFunctions;

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
                int dataX = 0;
                int dataY = 0;
                int dataZ = 0;

                if(terms != null && terms.Length >= 1 )
                {
                    if (terms != null && terms.Length >= 1 && terms.Contains("nolabel"))
                        includeLabel = false;
                    if (terms != null && terms.Length >= 1 && terms.Contains("nosecondary"))
                        secondaryRolls = false;
                    string foundTerm = terms.FirstOrDefault(a => a.StartsWith("x"));
                    if (foundTerm != null)
                    {
                        int.TryParse(foundTerm.Replace(" ", "").Replace("x=", ""), out dataX);
                    }
                    foundTerm = terms.FirstOrDefault(a => a.StartsWith("y"));
                    if (foundTerm != null)
                    {
                        int.TryParse(foundTerm.Replace(" ", "").Replace("y=", ""), out dataY);
                    }
                    foundTerm = terms.FirstOrDefault(a => a.StartsWith("z"));
                    if (foundTerm != null)
                    {
                        int.TryParse(foundTerm.Replace(" ", "").Replace("z=", ""), out dataZ);
                    }
                }

                int rollModifier = commandController.GetRollModifierFromCommandTerms(terms);
                string tableName = commandController.GetTableNameFromCommandTerms(terms);

                SavedRollTable savedTable = Utils.GetTableFromId(bot.SavedTables, tableName);

                if(savedTable.DefaultTable || thisChannel.AllowCustomTableRolls)
                {
                    string sendMessage = bot.DiceBot.GetRollTableResult(bot.SavedTables, tableName, characterName, channel, rollModifier, includeLabel, secondaryRolls, dataX, dataY, dataZ, DiceBot.MaximumSecondaryTableRolls);

                    bot.SendMessageInChannel(sendMessage, channel);
                }
                else
                {
                    bot.SendMessageInChannel("Only default tables are allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.", channel);
                }
            }
            else
            {
                bot.SendMessageInChannel(Name + " is currently not allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.", channel);
            }
        }
    }

    public class RollVariableData
    {
        public string Name;
        public int Value;
    }
}
