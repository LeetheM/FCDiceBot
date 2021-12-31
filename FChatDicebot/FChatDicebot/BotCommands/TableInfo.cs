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
    public class TableInfo : ChatBotCommand
    {
        public TableInfo()
        {
            Name = "tableinfo";
            RequireBotAdmin = false;
            RequireChannelAdmin = false;
            RequireChannel = true;
            LockCategory = CommandLockCategory.SavedTables;
        }

        public override void Run(BotMain bot, BotCommandController commandController, string[] rawTerms, string[] terms, string characterName, string channel, UserGeneratedCommand command)
        {
            ChannelSettings thisChannel = bot.GetChannelSettings(channel);

            if(thisChannel.AllowTableInfo)
            {
                string tableName = commandController.GetTableNameFromCommandTerms(terms);

                SavedRollTable infoTable = Utils.GetTableFromId(bot.SavedTables, tableName);

                string sendMessage = "Table \'" + tableName + "\' not found.";
                if (infoTable != null)
                {
                    sendMessage = "Table id [b]" + infoTable.TableId + "[/b] created by [user]" + infoTable.OriginCharacter + "[/user]";

                    if (infoTable.Table != null)
                    {
                        string tabledesc = infoTable.Table.Description + "\n";
                        sendMessage += "\n\n Name: [b]" + infoTable.Table.Name + "[/b]\n " + tabledesc + " Roll Die: d" + infoTable.Table.DieSides + " Roll Bonus: " + infoTable.Table.RollBonus;
                        sendMessage += "\n" + infoTable.Table.GetTableEntryList();
                    }
                    else
                    {
                        sendMessage += "\n (Rolltable contents not found)";
                    }
                }

                bot.SendMessageInChannel(sendMessage, channel);
            }
            else
            {
                bot.SendMessageInChannel(Name + " is currently not allowed in this channel under " + Utils.GetCharacterUserTags("Dice Bot") + "'s settings for this channel.", channel);
            }
            
        }
    }
}
